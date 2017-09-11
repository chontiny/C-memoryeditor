namespace Squalr.Source.Browse.Library
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using Squalr.Source.Output;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
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
        private ObservableCollection<Library> libraries;

        /// <summary>
        /// Prevents a default instance of the <see cref="LibraryViewModel" /> class from being created.
        /// </summary>
        private LibraryViewModel() : base("Library")
        {
            this.ContentId = LibraryViewModel.ToolContentId;

            this.SelectGameCommand = new RelayCommand<Game>((game) => this.SelectGame(game), (game) => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="LibraryViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static LibraryViewModel GetInstance()
        {
            return libraryViewModelInstance.Value;
        }

        /// <summary>
        /// Gets the command to select a game.
        /// </summary>
        public ICommand SelectGameCommand { get; private set; }

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
            }
        }

        /// <summary>
        /// Gets the list of libraries.
        /// </summary>
        public ObservableCollection<Library> Libraries
        {
            get
            {
                return this.libraries;
            }

            private set
            {
                this.libraries = value;
                this.RaisePropertyChanged(nameof(this.Libraries));
            }
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
                    BrowseViewModel.GetInstance().Navigate(BrowsePage.LibraryGameSelect);
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
            Task.Run(() =>
            {
                AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

                try
                {
                    this.GameList = SqualrApi.GetOwnedGameList(accessTokens?.AccessToken);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error fetching game list", ex);

                    BrowseViewModel.GetInstance().NavigateBack();
                }
            });
        }

        /// <summary>
        /// Selects a specific game for which to view the store.
        /// </summary>
        /// <param name="game">The selected game.</param>
        private void SelectGame(Game game)
        {
            BrowseViewModel.GetInstance().Navigate(BrowsePage.Loading);

            Task.Run(() =>
            {
                try
                {
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    Library[] libraries = SqualrApi.GetLibraries(accessTokens.AccessToken, game.GameId);
                    this.Libraries = new ObservableCollection<Library>(libraries);
                    BrowseViewModel.GetInstance().Navigate(BrowsePage.LibrarySelect);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading libraries", ex);
                    BrowseViewModel.GetInstance().Navigate(BrowsePage.StoreGameSelect);
                }
            });
        }
    }
    //// End class
}
//// End namespace