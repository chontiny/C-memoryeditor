namespace Squalr.Source.Browse.Library
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Controls;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using Squalr.Source.Output;
    using Squalr.Source.ProjectExplorer;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(LibraryViewModel);

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
        private List<Library> libraries;

        /// <summary>
        /// The list of cheats available to the library.
        /// </summary>
        private List<Cheat> cheatsAvailable;

        /// <summary>
        /// The list of cheats in the library.
        /// </summary>
        private List<Cheat> cheatsInLibrary;

        /// <summary>
        /// The selected game.
        /// </summary>
        private Game selectedGame;

        /// <summary>
        /// The selected library.
        /// </summary>
        private Library selectedLibrary;

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
            this.ContentId = LibraryViewModel.ToolContentId;
            this.libraries = new List<Library>();
            this.cheatsAvailable = new List<Cheat>();
            this.cheatsInLibrary = new List<Cheat>();

            this.SelectGameCommand = new RelayCommand<Game>((game) => this.SelectGame(game), (game) => true);
            this.ImportSelectedLibraryCommand = new RelayCommand(() => this.ImportSelectedLibrary(), () => true);
            this.SelectLibraryCommand = new RelayCommand<Library>((library) => this.SelectLibrary(library), (library) => true);
            this.AddCheatToLibraryCommand = new RelayCommand<Cheat>((cheat) => this.AddCheatToLibrary(cheat), (cheat) => true);
            this.RemoveCheatFromLibraryCommand = new RelayCommand<Cheat>((cheat) => this.RemoveCheatFromLibrary(cheat), (cheat) => true);
            this.AddNewLibraryCommand = new RelayCommand(() => this.AddNewLibrary(), () => true);
            this.DeleteLibraryCommand = new RelayCommand(() => this.DeleteLibrary(), () => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to select a game.
        /// </summary>
        public ICommand SelectGameCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a library.
        /// </summary>
        public ICommand SelectLibraryCommand { get; private set; }

        /// <summary>
        /// Gets the command to import the selected library.
        /// </summary>
        public ICommand ImportSelectedLibraryCommand { get; private set; }

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
            }
        }

        /// <summary>
        /// Gets or sets the selected library name.
        /// </summary>
        public String SelectedLibraryName
        {
            get
            {
                return this.SelectedLibrary?.LibraryName;
            }

            set
            {
                if (this.SelectedLibrary == null)
                {
                    return;
                }

                this.SelectedLibrary.LibraryName = value;

                // Cancel the previous API call if it exists
                this.CancellationTokenSource?.Cancel();
                this.CancellationTokenSource = new CancellationTokenSource();

                Task.Factory.StartNew(
                    (Object cancellationTokenSourceObject) =>
                    {
                        // When updating the library name in the API, put a cancellable delay
                        // This prevents the API from being called for every keystroke
                        Thread.Sleep(600);

                        CancellationTokenSource cancellationTokenSource = cancellationTokenSourceObject as CancellationTokenSource;

                        // Only perform the API if this task was not canceled
                        if (!cancellationTokenSource.IsCancellationRequested)
                        {
                            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                            SqualrApi.RenameLibrary(accessTokens.AccessToken, this.SelectedLibrary.LibraryId, this.SelectedLibrary.LibraryName);
                        }
                    },
                    this.CancellationTokenSource,
                    this.CancellationTokenSource.Token);

                this.RaisePropertyChanged(nameof(this.Libraries));
                this.RaisePropertyChanged(nameof(this.SelectedLibraryName));
            }
        }

        /// <summary>
        /// Gets or sets the selected library.
        /// </summary>
        public Library SelectedLibrary
        {
            get
            {
                return this.selectedLibrary;
            }

            set
            {
                this.selectedLibrary = value;
                this.RaisePropertyChanged(nameof(this.SelectedLibrary));
                this.RaisePropertyChanged(nameof(this.SelectedLibraryName));
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
                return this.SelectedLibrary != null;
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
        /// Gets the list of libraries.
        /// </summary>
        public ObservableCollection<Library> Libraries
        {
            get
            {
                return this.libraries == null ? null : new ObservableCollection<Library>(this.libraries);
            }
        }

        /// <summary>
        /// Gets the list of cheats available to the library.
        /// </summary>
        public ObservableCollection<Cheat> CheatsAvailable
        {
            get
            {
                return this.cheatsAvailable == null ? null : new ObservableCollection<Cheat>(this.cheatsAvailable);
            }
        }

        /// <summary>
        /// Gets the  list of cheats in the library.
        /// </summary>
        public ObservableCollection<Cheat> CheatsInLibrary
        {
            get
            {
                return this.cheatsInLibrary == null ? null : new ObservableCollection<Cheat>(this.cheatsInLibrary);
            }
        }

        /// <summary>
        /// Gets or sets the cancellation source for a running task.
        /// </summary>
        private CancellationTokenSource CancellationTokenSource { get; set; }

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
        public void OnNavigate(BrowsePage browsePage)
        {
            switch (browsePage)
            {
                case BrowsePage.LibraryHome:
                    BrowseViewModel.GetInstance().Navigate(BrowsePage.LibraryGameSelect, addCurrentPageToHistory: false);
                    break;
                case BrowsePage.LibraryGameSelect:
                    this.LoadGameList();
                    break;
                default:
                    return;
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
        /// Selects a specific game for which to view the store.
        /// </summary>
        /// <param name="game">The selected game.</param>
        private void SelectGame(Game game)
        {
            // Deselect current game
            this.SelectedGame = null;
            this.IsLibraryListLoading = true;
            this.SelectedLibrary = null;
            this.libraries = null;
            this.RaisePropertyChanged(nameof(this.Libraries));

            BrowseViewModel.GetInstance().Navigate(BrowsePage.LibrarySelect);

            Task.Run(() =>
            {
                try
                {
                    // Select new game
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    IEnumerable<Library> libraries = SqualrApi.GetLibraries(accessTokens.AccessToken, game.GameId);
                    this.libraries = new List<Library>(libraries);
                    this.RaisePropertyChanged(nameof(this.Libraries));
                    this.SelectedGame = game;
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading libraries", ex);
                    BrowseViewModel.GetInstance().Navigate(BrowsePage.StoreGameSelect);
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
            Task.Run(() =>
            {
                try
                {
                    this.IsLibraryLoading = true;

                    // Deselect current library
                    this.SelectedLibrary = null;
                    this.cheatsAvailable = null;
                    this.cheatsInLibrary = null;
                    this.RaisePropertyChanged(nameof(this.CheatsAvailable));
                    this.RaisePropertyChanged(nameof(this.CheatsInLibrary));

                    // Select library
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    LibraryCheats libraryCheats = SqualrApi.GetCheats(accessTokens.AccessToken, library.LibraryId);
                    this.SelectedLibrary = library;
                    this.cheatsAvailable = new List<Cheat>(libraryCheats.CheatsAvailable);
                    this.cheatsInLibrary = new List<Cheat>(libraryCheats.CheatsInLibrary);
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
        /// Imports the currently selected library.
        /// </summary>
        private void ImportSelectedLibrary()
        {
            if (this.SelectedLibrary == null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "No library selected");
            }

            if (this.cheatsInLibrary == null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error fetching cheats from selected library");
            }

            // Set imported library as the active library
            Task.Run(() =>
            {
                try
                {
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    SqualrApi.SetActiveLibrary(accessTokens?.AccessToken, this.SelectedLibrary.LibraryId);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error setting active library", ex);
                }
            });

            // Import the cheats
            ProjectExplorerViewModel.GetInstance().AddNewProjectItems(false, this.cheatsInLibrary.Select(x => x.ProjectItem).ToArray());
            ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
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
            if (this.SelectedLibrary == null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "No library selected");
                return;
            }

            MessageBoxResult result = CenteredDialogBox.Show(
                "Delete library '" + this.SelectedLibrary.LibraryName + "'?",
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
                        SqualrApi.DeleteLibrary(accessTokens?.AccessToken, this.SelectedLibrary.LibraryId);
                        this.libraries.Remove(this.SelectedLibrary);
                        this.SelectedLibrary = null;
                        this.RaisePropertyChanged(nameof(this.Libraries));
                        this.RaisePropertyChanged(nameof(this.SelectedLibrary));
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
                SqualrApi.AddCheatToLibrary(accessTokens.AccessToken, this.SelectedLibrary.LibraryId, cheat.CheatId);
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
                SqualrApi.RemoveCheatFromLibrary(accessTokens.AccessToken, this.SelectedLibrary.LibraryId, cheat.CheatId);

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