namespace Squalr.Source.Browse.Store
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
    /// View model for the Store.
    /// </summary>
    internal class StoreViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(StoreViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="StoreViewModel" /> class.
        /// </summary>
        private static Lazy<StoreViewModel> storeViewModelInstance = new Lazy<StoreViewModel>(
                () => { return new StoreViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The current store view.
        /// </summary>
        private StoreView currentView;

        /// <summary>
        /// The list of games.
        /// </summary>
        private IEnumerable<Game> gameList;

        /// <summary>
        /// The list of cheats in the store for the selected game.
        /// </summary>
        private ObservableCollection<Cheat> lockedCheatList;

        /// <summary>
        /// The list of purchased or owned cheats for the selected game.
        /// </summary>
        private ObservableCollection<Cheat> unlockedCheatList;

        /// <summary>
        /// Prevents a default instance of the <see cref="StoreViewModel" /> class from being created.
        /// </summary>
        private StoreViewModel() : base("Store")
        {
            this.ContentId = StoreViewModel.ToolContentId;
            this.CurrentView = StoreView.GameSelect;

            this.LockedCheatList = new ObservableCollection<Cheat>();
            this.UnlockedCheatList = new ObservableCollection<Cheat>();

            this.SelectGameCommand = new RelayCommand<Game>((game) => this.SelectGame(game), (game) => true);
            this.UnlockCheatCommand = new RelayCommand<Cheat>((cheat) => this.UnlockCheat(cheat), (cheat) => true);

            Task.Run(() =>
            {
                this.GameList = SqualrApi.GetGameList();
            });

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to select a game.
        /// </summary>
        public ICommand SelectGameCommand { get; private set; }

        /// <summary>
        /// Gets the command to unlock a cheat.
        /// </summary>
        public ICommand UnlockCheatCommand { get; private set; }

        /// <summary>
        /// Gets or sets the current store view.
        /// </summary>
        public StoreView CurrentView
        {
            get
            {
                return this.currentView;
            }

            set
            {
                this.currentView = value;
                this.RaisePropertyChanged(nameof(this.CurrentView));
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
            }
        }

        /// <summary>
        /// Gets or sets the list of cheats in the store for the selected game.
        /// </summary>
        public ObservableCollection<Cheat> LockedCheatList
        {
            get
            {
                return this.lockedCheatList;
            }

            set
            {
                this.lockedCheatList = value;
                this.RaisePropertyChanged(nameof(this.LockedCheatList));
            }
        }

        /// <summary>
        /// Gets or sets the list of cheats in the store for the selected game.
        /// </summary>
        public ObservableCollection<Cheat> UnlockedCheatList
        {
            get
            {
                return this.unlockedCheatList;
            }

            set
            {
                this.unlockedCheatList = value;
                this.RaisePropertyChanged(nameof(this.UnlockedCheatList));
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StoreViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StoreViewModel GetInstance()
        {
            return storeViewModelInstance.Value;
        }

        /// <summary>
        /// Selects a specific game for which to view the store.
        /// </summary>
        /// <param name="game">The selected game.</param>
        private void SelectGame(Game game)
        {
            this.CurrentView = StoreView.Loading;

            Task.Run(() =>
            {
                try
                {
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    StoreCheats storeCheats = SqualrApi.GetCheatList(accessTokens.AccessToken, game.GameId);
                    this.LockedCheatList = new ObservableCollection<Cheat>(storeCheats.LockedCheats);
                    this.UnlockedCheatList = new ObservableCollection<Cheat>(storeCheats.UnlockedCheats);
                    this.CurrentView = StoreView.CheatStore;
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading cheats", ex);
                    this.CurrentView = StoreView.GameSelect;
                }
            });
        }

        /// <summary>
        /// Attempts to unlock the provided cheat.
        /// </summary>
        /// <param name="cheat">The cheat to unlock</param>
        private void UnlockCheat(Cheat cheat)
        {
            if (!this.LockedCheatList.Contains(cheat))
            {
                throw new Exception("Cheat must be a locked cheat");
            }

            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

            // We need the unlocked cheat, since the locked one does not include the payload
            try
            {
                Cheat unlockedCheat = SqualrApi.UnlockCheat(accessTokens.AccessToken, cheat.CheatId);

                this.LockedCheatList.Remove(cheat);
                this.UnlockedCheatList.Insert(0, unlockedCheat);
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error unlocking cheat", ex);
            }
        }
    }
    //// End class
}
//// End namespace