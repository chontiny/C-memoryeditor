namespace Squalr.Source.Browse
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Browser.
    /// </summary>
    internal class BrowseViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(BrowseViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="BrowseViewModel" /> class.
        /// </summary>
        private static Lazy<BrowseViewModel> browseViewModelInstance = new Lazy<BrowseViewModel>(
                () => { return new BrowseViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A value indicating whether the user is logged into the site.
        /// </summary>
        private Boolean isLoggedIn;

        /// <summary>
        /// The current browse view.
        /// </summary>
        private BrowseView currentView;

        /// <summary>
        /// Prevents a default instance of the <see cref="BrowseViewModel" /> class from being created.
        /// </summary>
        private BrowseViewModel() : base("Browse")
        {
            this.ContentId = BrowseViewModel.ToolContentId;

            this.OpenLoginScreenCommand = new RelayCommand(() => this.CurrentView = BrowseView.Login, () => true);
            this.OpenStoreCommand = new RelayCommand(() => this.CurrentView = BrowseView.Store, () => true);
            this.OpenLibraryCommand = new RelayCommand(() => this.CurrentView = BrowseView.Library, () => true);
            this.OpenStreamCommand = new RelayCommand(() => this.CurrentView = BrowseView.Stream, () => true);

            Task.Run(() => this.SetDefaultViewOptions());

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="BrowseViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static BrowseViewModel GetInstance()
        {
            return browseViewModelInstance.Value;
        }

        /// <summary>
        /// Gets a command to open the login screen.
        /// </summary>
        public ICommand OpenLoginScreenCommand { get; private set; }

        /// <summary>
        /// Gets a command to open the store.
        /// </summary>
        public ICommand OpenStoreCommand { get; private set; }

        /// <summary>
        /// Gets a command to open the library.
        /// </summary>
        public ICommand OpenLibraryCommand { get; private set; }

        /// <summary>
        /// Gets a command to open the stream config.
        /// </summary>
        public ICommand OpenStreamCommand { get; private set; }

        /// <summary>
        /// Gets or sets the current browse view.
        /// </summary>
        public BrowseView CurrentView
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
        /// Gets or sets a value indicating whether the user is logged into the site.
        /// </summary>
        public Boolean IsLoggedIn
        {
            get
            {
                return this.isLoggedIn;
            }

            set
            {
                this.isLoggedIn = value;
                this.RaisePropertyChanged(nameof(this.IsLoggedIn));
            }
        }

        /// <summary>
        /// Sets the default view to show, ie login screen or store.
        /// </summary>
        private void SetDefaultViewOptions()
        {
            if (!this.IsLoggedIn)
            {
                this.UpdateLoginStatus();
                this.CurrentView = BrowseView.Login;
            }
            else
            {
                this.CurrentView = BrowseView.Store;
            }
        }

        /// <summary>
        /// Determine if the user is logged in to Twitch.
        /// </summary>
        private void UpdateLoginStatus()
        {
            TwitchAccessTokens twitchAccessTokens = SettingsViewModel.GetInstance().TwitchAccessTokens;

            try
            {
                TwitchUser twitchUser = SqualrApi.GetTwitchUser(twitchAccessTokens.AccessToken);
                this.IsLoggedIn = true;
                this.CurrentView = BrowseView.Store;
            }
            catch
            {
                this.IsLoggedIn = false;
            }
        }
    }
    //// End class
}
//// End namespace