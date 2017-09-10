namespace Squalr.Source.Browse.Store
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using System;
    using System.Collections.Generic;
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
        /// The list of cheats in the store for the selected game.
        /// </summary>
        private IEnumerable<Cheat> lockedCheatList;

        /// <summary>
        /// The list of purchased or owned cheats for the selected game.
        /// </summary>
        private IEnumerable<Cheat> unlockedCheatList;

        /// <summary>
        /// Prevents a default instance of the <see cref="StoreViewModel" /> class from being created.
        /// </summary>
        private StoreViewModel() : base("Store")
        {
            this.ContentId = StoreViewModel.ToolContentId;
            this.CurrentView = StoreView.GameSelect;

            this.SelectGameCommand = new RelayCommand<Game>((game) => this.SelectGame(game), (game) => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to connect to the stream.
        /// </summary>
        public ICommand SelectGameCommand { get; private set; }

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
        /// Gets or sets the list of cheats in the store for the selected game.
        /// </summary>
        public IEnumerable<Cheat> LockedCheatList
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
        public IEnumerable<Cheat> UnlockedCheatList
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
            this.CurrentView = StoreView.CheatStore;

            TwitchAccessTokens twitchAccessTokens = SettingsViewModel.GetInstance().TwitchAccessTokens;

            Task.Run(() =>
            {
                StoreCheats storeCheats = SqualrApi.GetCheatList(twitchAccessTokens.AccessToken, game.GameId);
                this.LockedCheatList = storeCheats.LockedCheats;
                this.UnlockedCheatList = storeCheats.UnlockedCheats;
            });
        }
    }
    //// End class
}
//// End namespace