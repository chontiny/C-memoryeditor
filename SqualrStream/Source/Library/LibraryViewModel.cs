namespace SqualrStream.Source.Library
{
    using GalaSoft.MvvmLight.CommandWpf;
    using SqualrCore.Source.Controls;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.PropertyViewer;
    using SqualrCore.Source.Utils.DataStructures;
    using SqualrStream.Properties;
    using SqualrStream.Source.Api;
    using SqualrStream.Source.Api.Models;
    using SqualrStream.Source.Navigation;
    using SqualrStream.Source.Store;
    using SqualrStream.Source.Stream;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Library.
    /// </summary>
    internal class LibraryViewModel : ToolViewModel, INavigable
    {
        /// <summary>
        /// Singleton instance of the <see cref="StoreViewModel" /> class.
        /// </summary>
        private static Lazy<LibraryViewModel> libraryViewModelInstance = new Lazy<LibraryViewModel>(
                () => { return new LibraryViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The list of games.
        /// </summary>
        private IEnumerable<Game> gameList;

        /// <summary>
        /// The list of libraries.
        /// </summary>
        private FullyObservableCollection<Library> libraries;

        /// <summary>
        /// The list of cheats available to the library.
        /// </summary>
        private FullyObservableCollection<Cheat> cheatsAvailable;

        /// <summary>
        /// The list of cheats in the library.
        /// </summary>
        private FullyObservableCollection<Cheat> cheatsInLibrary;

        /// <summary>
        /// The selected game.
        /// </summary>
        private Game selectedGame;

        /// <summary>
        /// The active library.
        /// </summary>
        private Library activeLibrary;

        /// <summary>
        /// A value indicating whether the game list is loading.
        /// </summary>
        private Boolean isGameListLoading;

        /// <summary>
        /// A value indicating if the cheats from the library are loading.
        /// </summary>
        private Boolean isLibraryLoading;

        /// <summary>
        /// A value indicating whether the library list is loading.
        /// </summary>
        private Boolean isLibraryListLoading;

        /// <summary>
        /// The current search term.
        /// </summary>
        private String searchTerm;

        /// <summary>
        /// Prevents a default instance of the <see cref="LibraryViewModel" /> class from being created.
        /// </summary>
        private LibraryViewModel() : base("Library")
        {
            this.Libraries = new FullyObservableCollection<Library>();
            this.CheatsAvailable = new FullyObservableCollection<Cheat>();
            this.CheatsInLibrary = new FullyObservableCollection<Cheat>();

            this.SelectCheatCommand = new RelayCommand<Cheat>((cheat) => this.SelectCheat(cheat), (cheat) => true);
            this.SelectGameCommand = new RelayCommand<Game>((game) => this.SelectGame(game), (game) => true);
            this.SelectLibraryCommand = new RelayCommand<Library>((library) => this.SelectLibrary(library), (library) => true);
            this.AddCheatToLibraryCommand = new RelayCommand<Cheat>((cheat) => this.AddCheatToLibrary(cheat), (cheat) => true);
            this.RemoveCheatFromLibraryCommand = new RelayCommand<Cheat>((cheat) => this.RemoveCheatFromLibrary(cheat), (cheat) => true);
            this.AddNewLibraryCommand = new RelayCommand(() => this.AddNewLibrary(), () => true);
            this.DeleteLibraryCommand = new RelayCommand(() => this.DeleteLibrary(), () => true);

            this.CheatUpdateTask = new CheatUpdateTask(this.UpdateCheats);

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets the command to select a cheat.
        /// </summary>
        public ICommand SelectCheatCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a game.
        /// </summary>
        public ICommand SelectGameCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a library.
        /// </summary>
        public ICommand SelectLibraryCommand { get; private set; }

        /// <summary>
        /// Gets the command to create a new library.
        /// </summary>
        public ICommand AddNewLibraryCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete the selected library.
        /// </summary>
        public ICommand DeleteLibraryCommand { get; private set; }

        /// <summary>
        /// Gets the command to remove a cheat from the library.
        /// </summary>
        public ICommand AddCheatToLibraryCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a cheat to the library.
        /// </summary>
        public ICommand RemoveCheatFromLibraryCommand { get; private set; }

        /// <summary>
        /// Gets or sets the current search term.
        /// </summary>
        public String SearchTerm
        {
            get
            {
                return this.searchTerm;
            }

            set
            {
                this.searchTerm = value;
                this.RaisePropertyChanged(nameof(this.SearchTerm));
                this.RaisePropertyChanged(nameof(this.FilteredGameList));
            }
        }

        /// <summary>
        /// Gets the filtered list of games.
        /// </summary>
        public IEnumerable<Game> FilteredGameList
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this.SearchTerm))
                {
                    return this.GameList;
                }

                return this.GameList?
                    .Select(item => item)
                    .Where(item => item.GameName.IndexOf(this.SearchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0);
            }
        }

        /// <summary>
        /// Gets the list of games.
        /// </summary>
        public IEnumerable<Game> GameList
        {
            get
            {
                return this.gameList;
            }

            private set
            {
                this.gameList = value;
                this.RaisePropertyChanged(nameof(this.GameList));
                this.RaisePropertyChanged(nameof(this.FilteredGameList));
            }
        }

        /// <summary>
        /// Gets or sets the selected game.
        /// </summary>
        public Game SelectedGame
        {
            get
            {
                return this.selectedGame;
            }

            set
            {
                this.selectedGame = value;
                this.RaisePropertyChanged(nameof(this.SelectedGame));
                this.RaisePropertyChanged(nameof(this.BannerText));
            }
        }

        /// <summary>
        /// Gets or sets the active library name.
        /// </summary>
        public String ActiveLibraryName
        {
            get
            {
                return this.ActiveLibrary?.LibraryName;
            }

            set
            {
                if (this.ActiveLibrary == null)
                {
                    return;
                }

                this.ActiveLibrary.LibraryName = value;

                // Cancel the previous API call if it exists
                this.CancellationTokenSource?.Cancel();
                this.CancellationTokenSource = new CancellationTokenSource();

                Task.Factory.StartNew(
                    (Object cancellationTokenSourceObject) =>
                    {
                        // When updating the library name in the API, put a cancellable delay
                        // This prevents the API from being called for every keystroke
                        Thread.Sleep(1000);

                        CancellationTokenSource cancellationTokenSource = cancellationTokenSourceObject as CancellationTokenSource;

                        // Only perform the API if this task was not canceled
                        if (!cancellationTokenSource.IsCancellationRequested)
                        {
                            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                            SqualrApi.RenameLibrary(accessTokens.AccessToken, this.ActiveLibrary.LibraryId, this.ActiveLibrary.LibraryName);
                        }
                    },
                    this.CancellationTokenSource,
                    this.CancellationTokenSource.Token);

                this.Libraries.Select(x => x).Where(x => x.LibraryId == this.ActiveLibrary.LibraryId).Select(x => x.LibraryName = this.ActiveLibrary.LibraryName);

                this.RaisePropertyChanged(nameof(this.BannerText));
                this.RaisePropertyChanged(nameof(this.ActiveLibrary));
                this.RaisePropertyChanged(nameof(this.ActiveLibraryName));
            }
        }

        /// <summary>
        /// Gets or sets the active library.
        /// </summary>
        public Library ActiveLibrary
        {
            get
            {
                return this.activeLibrary;
            }

            set
            {
                this.activeLibrary = value;
                this.RaisePropertyChanged(nameof(this.ActiveLibrary));
                this.RaisePropertyChanged(nameof(this.BannerText));
                this.RaisePropertyChanged(nameof(this.ActiveLibraryName));
                this.RaisePropertyChanged(nameof(this.IsLibrarySelected));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is a selected library.
        /// </summary>
        public Boolean IsLibrarySelected
        {
            get
            {
                return this.ActiveLibrary != null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the game list is loading.
        /// </summary>
        public Boolean IsGameListLoading
        {
            get
            {
                return this.isGameListLoading;
            }

            set
            {
                this.isGameListLoading = value;
                this.RaisePropertyChanged(nameof(this.IsGameListLoading));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cheats from the library are loading.
        /// </summary>
        public Boolean IsLibraryLoading
        {
            get
            {
                return this.isLibraryLoading;
            }

            set
            {
                this.isLibraryLoading = value;
                this.RaisePropertyChanged(nameof(this.IsLibraryLoading));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the library list is loading.
        /// </summary>
        public Boolean IsLibraryListLoading
        {
            get
            {
                return this.isLibraryListLoading;
            }

            set
            {
                this.isLibraryListLoading = value;
                this.RaisePropertyChanged(nameof(this.IsLibraryListLoading));
            }
        }

        /// <summary>
        /// Gets or sets the list of libraries.
        /// </summary>
        public FullyObservableCollection<Library> Libraries
        {
            get
            {
                return this.libraries;
            }

            set
            {
                this.libraries = value;
                this.RaisePropertyChanged(nameof(this.Libraries));
            }
        }

        /// <summary>
        /// Gets or sets the list of cheats available to the library.
        /// </summary>
        public FullyObservableCollection<Cheat> CheatsAvailable
        {
            get
            {
                return this.cheatsAvailable;
            }

            set
            {
                this.cheatsAvailable = value;
                this.RaisePropertyChanged(nameof(this.CheatsAvailable));
            }
        }

        /// <summary>
        /// Gets or sets the  list of cheats in the library.
        /// </summary>
        public FullyObservableCollection<Cheat> CheatsInLibrary
        {
            get
            {
                return this.cheatsInLibrary;
            }

            set
            {
                this.cheatsInLibrary = value;
                this.RaisePropertyChanged(nameof(this.CheatsInLibrary));
            }
        }

        /// <summary>
        /// Gets the banner text.
        /// </summary>
        public String BannerText
        {
            get
            {
                switch (BrowseViewModel.GetInstance().CurrentPage)
                {
                    case NavigationPage.GameSelect:
                        return "Select a Game".ToUpper();
                    case NavigationPage.LibrarySelect:
                        return (this.SelectedGame?.GameName)?.ToUpper();
                    case NavigationPage.LibraryEdit:
                        return (this.ActiveLibrary?.LibraryName)?.ToUpper();
                    case NavigationPage.Login:
                    default:
                        return String.Empty;

                }
            }
        }

        /// <summary>
        /// Gets or sets the cancellation source for a running task.
        /// </summary>
        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets or sets the task to update all cheats.
        /// </summary>
        private CheatUpdateTask CheatUpdateTask { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="LibraryViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static LibraryViewModel GetInstance()
        {
            return libraryViewModelInstance.Value;
        }

        /// <summary>
        /// Event fired when the browse view navigates to a new page.
        /// </summary>
        /// <param name="browsePage">The new browse page.</param>
        public void OnNavigate(NavigationPage browsePage)
        {
            this.RaisePropertyChanged(nameof(this.BannerText));

            switch (browsePage)
            {
                case NavigationPage.GameSelect:
                    this.LoadGameList();
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Event fired when a cheat is unlocked and made available to this library.
        /// </summary>
        /// <param name="unlockedCheat">The cheat that was unlocked.</param>
        public void OnUnlock(Cheat unlockedCheat)
        {
            this.CheatsAvailable.Insert(0, unlockedCheat);
            this.RaisePropertyChanged(nameof(this.CheatsAvailable));
        }

        /// <summary>
        /// Event fired to update all cheats.
        /// </summary>
        private void UpdateCheats()
        {
            Cheat[] cheats = this.CheatsInLibrary?.ToArray();

            if (cheats == null)
            {
                return;
            }

            foreach (Cheat cheat in cheats)
            {
                cheat.Update();
            }
        }

        /// <summary>
        /// Loads the list of games from the API.
        /// </summary>
        private void LoadGameList()
        {
            this.IsGameListLoading = true;
            this.GameList = null;

            Task.Run(() =>
            {
                try
                {
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    this.GameList = SqualrApi.GetOwnedGameList(accessTokens.AccessToken);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error fetching game list", ex);

                    BrowseViewModel.GetInstance().NavigateBack();
                }
                finally
                {
                    this.IsGameListLoading = false;
                }
            });
        }

        /// <summary>
        /// Selects a given cheat.
        /// </summary>
        /// <param name="cheat">The cheat to select</param>
        private void SelectCheat(Cheat cheat)
        {
            PropertyViewerViewModel.GetInstance().SetTargetObjects(cheat);
        }

        /// <summary>
        /// Selects a specific game for which to view the store.
        /// </summary>
        /// <param name="game">The selected game.</param>
        private void SelectGame(Game game)
        {
            // Deselect current game
            this.SelectedGame = null;
            this.Libraries = null;
            this.IsLibraryListLoading = true;
            this.ActiveLibrary = null;
            this.RaisePropertyChanged(nameof(this.Libraries));

            BrowseViewModel.GetInstance().Navigate(NavigationPage.LibrarySelect);

            Task.Run(() =>
            {
                try
                {
                    StoreViewModel.GetInstance().SelectGame(game);

                    // Select new game
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    IEnumerable<Library> libraries = SqualrApi.GetLibraries(accessTokens.AccessToken, game.GameId);

                    this.Libraries = new FullyObservableCollection<Library>(libraries);
                    this.RaisePropertyChanged(nameof(this.Libraries));

                    this.SelectedGame = game;
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading libraries", ex);
                    BrowseViewModel.GetInstance().Navigate(NavigationPage.GameSelect);
                }
                finally
                {
                    this.IsLibraryListLoading = false;
                }
            });
        }

        /// <summary>
        /// Selects a specific game for which to view the store.
        /// </summary>
        /// <param name="library">The selected library.</param>
        private void SelectLibrary(Library library)
        {
            BrowseViewModel.GetInstance().Navigate(NavigationPage.LibraryEdit);

            Task.Run(() =>
            {
                try
                {
                    this.IsLibraryLoading = true;

                    // Deselect current library
                    this.ActiveLibrary = null;
                    this.CheatsAvailable = null;
                    this.CheatsInLibrary = null;
                    this.RaisePropertyChanged(nameof(this.CheatsAvailable));
                    this.RaisePropertyChanged(nameof(this.CheatsInLibrary));

                    // Select library
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    LibraryCheats libraryCheats = SqualrApi.GetCheats(accessTokens.AccessToken, library.LibraryId);
                    SqualrApi.SetActiveLibrary(accessTokens.AccessToken, library.LibraryId);

                    this.ActiveLibrary = library;
                    this.CheatsAvailable = new FullyObservableCollection<Cheat>(libraryCheats.CheatsAvailable);
                    this.CheatsInLibrary = new FullyObservableCollection<Cheat>(libraryCheats.CheatsInLibrary);
                    this.RaisePropertyChanged(nameof(this.CheatsAvailable));
                    this.RaisePropertyChanged(nameof(this.CheatsInLibrary));
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading cheats", ex);
                }
                finally
                {
                    this.IsLibraryLoading = false;
                }
            });
        }

        /// <summary>
        /// Adds a new library to the current selected game.
        /// </summary>
        private void AddNewLibrary()
        {
            Task.Run(() =>
            {
                try
                {
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    Library newLibrary = SqualrApi.CreateLibrary(accessTokens?.AccessToken, this.SelectedGame.GameId);
                    this.libraries.Add(newLibrary);
                    this.RaisePropertyChanged(nameof(this.Libraries));
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error creating library", ex);
                }
            });
        }

        /// <summary>
        /// Deletes the selected library after prompting the user.
        /// </summary>
        private void DeleteLibrary()
        {
            if (this.ActiveLibrary == null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "No library selected");
                return;
            }

            MessageBoxResult result = CenteredDialogBox.Show(
                "Delete library '" + this.ActiveLibrary.LibraryName + "'?",
                "Confirm Library Delete",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Task.Run(() =>
                {
                    try
                    {
                        AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                        SqualrApi.DeleteLibrary(accessTokens?.AccessToken, this.ActiveLibrary.LibraryId);
                        this.libraries.Remove(this.ActiveLibrary);
                        this.ActiveLibrary = null;
                        this.RaisePropertyChanged(nameof(this.Libraries));
                        this.RaisePropertyChanged(nameof(this.ActiveLibrary));
                    }
                    catch (Exception ex)
                    {
                        OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error deleting library", ex);
                    }
                });
            }
        }

        /// <summary>
        /// Adds the cheat to the selected library.
        /// </summary>
        /// <param name="cheat">The cheat to add.</param>
        private void AddCheatToLibrary(Cheat cheat)
        {
            if (!this.CheatsAvailable.Contains(cheat))
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to add cheat to library");
                return;
            }

            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

            try
            {
                SqualrApi.AddCheatToLibrary(accessTokens.AccessToken, this.ActiveLibrary.LibraryId, cheat.CheatId);

                cheat.LoadDefaultStreamSettings();

                this.cheatsAvailable.Remove(cheat);
                this.cheatsInLibrary.Insert(0, cheat);
                this.RaisePropertyChanged(nameof(this.CheatsAvailable));
                this.RaisePropertyChanged(nameof(this.CheatsInLibrary));
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error adding cheat to library", ex);
            }
        }

        /// <summary>
        /// Removes the cheat from the selected library.
        /// </summary>
        /// <param name="cheat">The cheat to remove.</param>
        private void RemoveCheatFromLibrary(Cheat cheat)
        {
            if (!this.CheatsInLibrary.Contains(cheat))
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to remove cheat from library");
                return;
            }

            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

            try
            {
                SqualrApi.RemoveCheatFromLibrary(accessTokens.AccessToken, this.ActiveLibrary.LibraryId, cheat.CheatId);

                this.cheatsInLibrary.Remove(cheat);
                this.cheatsAvailable.Insert(0, cheat);
                this.RaisePropertyChanged(nameof(this.CheatsAvailable));
                this.RaisePropertyChanged(nameof(this.CheatsInLibrary));
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error removing cheat from library", ex);
            }
        }
    }
    //// End class
}
//// End namespace