namespace SqualrStream.Source.Store
{
    using GalaSoft.MvvmLight.CommandWpf;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.DataStructures;
    using SqualrStream.Properties;
    using SqualrStream.Source.Api;
    using SqualrStream.Source.Api.Models;
    using SqualrStream.Source.Library;
    using SqualrStream.Source.Navigation;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Store.
    /// </summary>
    internal class StoreViewModel : ToolViewModel, INavigable
    {
        /// <summary>
        /// Singleton instance of the <see cref="StoreViewModel" /> class.
        /// </summary>
        private static Lazy<StoreViewModel> storeViewModelInstance = new Lazy<StoreViewModel>(
                () => { return new StoreViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The list of cheats in the store for the selected game.
        /// </summary>
        private FullyObservableCollection<Cheat> lockedCheatList;

        /// <summary>
        /// The list of purchased or owned cheats for the selected game.
        /// </summary>
        private FullyObservableCollection<Cheat> unlockedCheatList;

        /// <summary>
        /// A value indicating whether the cheat list is loading.
        /// </summary>
        private Boolean isCheatListLoading;

        /// <summary>
        /// Prevents a default instance of the <see cref="StoreViewModel" /> class from being created.
        /// </summary>
        private StoreViewModel() : base("Store")
        {
            this.LockedCheatList = new FullyObservableCollection<Cheat>();
            this.UnlockedCheatList = new FullyObservableCollection<Cheat>();

            this.UnlockCheatCommand = new RelayCommand<Cheat>((cheat) => this.UnlockCheat(cheat), (cheat) => true);

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets the command to unlock a cheat.
        /// </summary>
        public ICommand UnlockCheatCommand { get; private set; }

        /// <summary>
        /// Gets or sets the list of cheats in the store for the selected game.
        /// </summary>
        public FullyObservableCollection<Cheat> LockedCheatList
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
        public FullyObservableCollection<Cheat> UnlockedCheatList
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
        /// Gets or sets a value indicating whether the cheat list is loading.
        /// </summary>
        public Boolean IsCheatListLoading
        {
            get
            {
                return this.isCheatListLoading;
            }

            set
            {
                this.isCheatListLoading = value;
                this.RaisePropertyChanged(nameof(this.IsCheatListLoading));
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
        /// Event fired when the browse view navigates to a new page.
        /// </summary>
        /// <param name="browsePage">The new browse page.</param>
        public void OnNavigate(NavigationPage browsePage)
        {
            this.RaisePropertyChanged(nameof(this.LockedCheatList));
            this.RaisePropertyChanged(nameof(this.UnlockedCheatList));

            switch (browsePage)
            {
                default:
                    return;
            }
        }

        /// <summary>
        /// Selects a specific game for which to view the store.
        /// </summary>
        /// <param name="game">The selected game.</param>
        public void SelectGame(Game game)
        {
            // Deselect current game
            this.IsCheatListLoading = true;
            this.LockedCheatList = null;
            this.UnlockedCheatList = null;

            Task.Run(() =>
            {
                try
                {
                    // Select new game
                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                    StoreCheats storeCheats = SqualrApi.GetStoreCheats(accessTokens.AccessToken, game.GameId);
                    this.LockedCheatList = new FullyObservableCollection<Cheat>(storeCheats.LockedCheats);
                    this.UnlockedCheatList = new FullyObservableCollection<Cheat>(storeCheats.UnlockedCheats);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading cheats", ex);
                    BrowseViewModel.GetInstance().NavigateBack();
                }
                finally
                {
                    this.IsCheatListLoading = false;
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
                UnlockedCheat unlockedCheat = SqualrApi.UnlockCheat(accessTokens.AccessToken, cheat.CheatId);

                BrowseViewModel.GetInstance().SetCoinAmount(unlockedCheat.RemainingCoins);

                this.LockedCheatList.Remove(cheat);
                this.UnlockedCheatList.Insert(0, unlockedCheat.Cheat);
                LibraryViewModel.GetInstance().OnUnlock(unlockedCheat.Cheat);
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