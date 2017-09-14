namespace Squalr.Source.Browse
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Browse.Library;
    using Squalr.Source.Browse.Store;
    using Squalr.Source.Browse.StreamConfig;
    using Squalr.Source.Browse.TwitchLogin;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using Squalr.Source.Output;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
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
        /// The active user.
        /// </summary>
        private User activeUser;

        /// <summary>
        /// The current browse page.
        /// </summary>
        private BrowsePage currentPage;

        /// <summary>
        /// Prevents a default instance of the <see cref="BrowseViewModel" /> class from being created.
        /// </summary>
        private BrowseViewModel() : base("Browse")
        {
            this.ContentId = BrowseViewModel.ToolContentId;
            this.PreviousPages = new Stack<BrowsePage>();
            this.NextPages = new Stack<BrowsePage>();
            this.AccessLock = new Object();

            this.OpenLoginScreenCommand = new RelayCommand(() => this.Navigate(BrowsePage.Login), () => true);
            this.LogoutCommand = new RelayCommand(() => this.Logout(), () => true);
            this.OpenVirtualCurrencyStoreCommand = new RelayCommand(() => this.OpenVirtualCurrencyStore(), () => true);
            this.OpenStoreCommand = new RelayCommand(() => this.Navigate(BrowsePage.StoreHome), () => true);
            this.OpenLibraryCommand = new RelayCommand(() => this.Navigate(BrowsePage.LibraryHome), () => true);
            this.OpenStreamCommand = new RelayCommand(() => this.Navigate(BrowsePage.StreamHome), () => true);
            this.NavigateForwardCommand = new RelayCommand(() => this.NavigateForward());
            this.NavigateBackCommand = new RelayCommand(() => this.NavigateBack());

            this.InitializeObservers();
            this.Navigate(BrowsePage.Loading);

            Task.Run(() =>
            {
                this.UpdateLoginStatus();
                this.SetDefaultViewOptions();
            });

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
        /// Gets a command to log out.
        /// </summary>
        public ICommand LogoutCommand { get; private set; }

        /// <summary>
        /// Gets a command to open the coin store.
        /// </summary>
        public ICommand OpenVirtualCurrencyStoreCommand { get; private set; }

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
        /// Gets a command to navigate the browse menu forward.
        /// </summary>
        public ICommand NavigateForwardCommand { get; private set; }

        /// <summary>
        /// Gets a command to navigate the browse menu backwards.
        /// </summary>
        public ICommand NavigateBackCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether navigate forwards is available.
        /// </summary>
        public Boolean IsForwardAvailable
        {
            get
            {
                return this.NextPages.Count > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether navigate backwards is available.
        /// </summary>
        public Boolean IsBackAvailable
        {
            get
            {
                return this.PreviousPages.Count > 0;
            }
        }

        /// <summary>
        /// Gets the banner text.
        /// </summary>
        public String BannerText
        {
            get
            {
                switch (this.CurrentCategory)
                {
                    case BrowseCategory.Loading:
                        return "Loading".ToUpper();
                    case BrowseCategory.Library:
                        return "Library".ToUpper();
                    case BrowseCategory.Store:
                        return "Store".ToUpper();
                    case BrowseCategory.Stream:
                        return "Stream".ToUpper();
                    case BrowseCategory.Login:
                    default:
                        return "Login".ToUpper();

                }
            }
        }

        /// <summary>
        /// Gets or sets the current browse section.
        /// </summary>
        public BrowsePage CurrentPage
        {
            get
            {
                return this.currentPage;
            }

            set
            {
                this.currentPage = value;
                this.RaisePropertyChanged(nameof(this.CurrentPage));
                this.RaisePropertyChanged(nameof(this.CurrentCategory));
                this.RaisePropertyChanged(nameof(this.BannerText));
            }
        }

        /// <summary>
        /// Gets the current browse category based on the current page.
        /// </summary>
        public BrowseCategory CurrentCategory
        {
            get
            {
                switch (this.CurrentPage)
                {
                    case BrowsePage.LibraryHome:
                    case BrowsePage.LibrarySelect:
                    case BrowsePage.LibraryGameSelect:
                        return BrowseCategory.Library;
                    case BrowsePage.StreamHome:
                        return BrowseCategory.Stream;
                    case BrowsePage.StoreHome:
                    case BrowsePage.StoreGameSelect:
                    case BrowsePage.CheatStore:
                        return BrowseCategory.Store;
                    case BrowsePage.Loading:
                        return BrowseCategory.Loading;
                    case BrowsePage.Login:
                    default:
                        return BrowseCategory.Login;
                }
            }
        }

        /// <summary>
        /// Gets or sets the active user.
        /// </summary>
        public User ActiveUser
        {
            get
            {
                return this.activeUser;
            }

            set
            {
                this.activeUser = value;
                this.RaisePropertyChanged(nameof(this.ActiveUser));
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
                this.SetDefaultViewOptions();
                this.RaisePropertyChanged(nameof(this.IsLoggedIn));
            }
        }

        /// <summary>
        /// Gets or sets the list of observers for navigation events.
        /// </summary>
        private IEnumerable<INavigable> Observers { get; set; }

        /// <summary>
        /// Gets or sets the previous pages in the browse history.
        /// </summary>
        private Stack<BrowsePage> PreviousPages { get; set; }

        /// <summary>
        /// Gets or sets the next pages in the browse history.
        /// </summary>
        private Stack<BrowsePage> NextPages { get; set; }

        /// <summary>
        /// Gets or sets the access lock to page history.
        /// </summary>
        private Object AccessLock { get; set; }

        /// <summary>
        /// Navigates the Browse view to the specified page.
        /// </summary>
        /// <param name="newPage">The page to which to navigate.</param>
        public void Navigate(BrowsePage newPage, Boolean addCurrentPageToHistory = true)
        {
            if (this.CurrentPage == newPage)
            {
                return;
            }

            lock (this.AccessLock)
            {
                // Save current page in history (not including loading or login pages)
                if (addCurrentPageToHistory && this.CurrentPage != BrowsePage.Loading && this.CurrentPage != BrowsePage.Login)
                {
                    this.NextPages.Clear();
                    this.PreviousPages.Push(this.CurrentPage);
                }

                this.CurrentPage = newPage;
                this.RaisePropertyChanged(nameof(this.IsBackAvailable));
                this.RaisePropertyChanged(nameof(this.IsForwardAvailable));
                Task.Run(() => this.Observers.ForEach(observer => observer.OnNavigate(newPage)));
            }
        }

        /// <summary>
        /// Navigates the Browse view backwards to a previosly visited page.
        /// </summary>
        public void NavigateBack()
        {
            BrowsePage previousPage;

            lock (this.AccessLock)
            {
                if (this.PreviousPages.Count == 0)
                {
                    return;
                }

                previousPage = this.PreviousPages.Pop();
                this.NextPages.Push(this.CurrentPage);
            }

            this.Navigate(previousPage, addCurrentPageToHistory: false);
        }

        /// <summary>
        /// Navigates the Browse view forwards to a previously visited page.
        /// </summary>
        public void NavigateForward()
        {
            BrowsePage nextPage;

            lock (this.AccessLock)
            {
                if (this.NextPages.Count == 0)
                {
                    return;
                }

                nextPage = this.NextPages.Pop();
                this.PreviousPages.Push(this.CurrentPage);
            }

            this.Navigate(nextPage, addCurrentPageToHistory: false);
        }

        /// <summary>
        /// Opens the virtual currency store in the native browser.
        /// </summary>
        private void OpenVirtualCurrencyStore()
        {
            NativeBrowser.Open(SqualrApi.VirtualCurrencyStoreEndpoint);
        }

        /// <summary>
        /// Sets the default view to show, ie login screen or store.
        /// </summary>
        private void SetDefaultViewOptions()
        {
            if (!this.IsLoggedIn)
            {
                this.Navigate(BrowsePage.Login);
            }
            else
            {
                this.Navigate(BrowsePage.StoreHome);
            }
        }

        /// <summary>
        /// Determine if the user is logged in to Twitch.
        /// </summary>
        private void UpdateLoginStatus()
        {
            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

            if (accessTokens == null || accessTokens.AccessToken.IsNullOrEmpty())
            {
                this.IsLoggedIn = false;
                return;
            }

            try
            {
                User user = SqualrApi.GetTwitchUser(accessTokens.AccessToken);
                this.ActiveUser = user;

                this.IsLoggedIn = true;
                this.Navigate(BrowsePage.StoreHome);
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to log in using stored credentials", ex);
                this.IsLoggedIn = false;
            }
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        private void Logout()
        {
            SettingsViewModel.GetInstance().AccessTokens = null;
            Settings.Default.Save();
            this.Navigate(BrowsePage.Login);
        }

        /// <summary>
        /// Initializes observers that listen for navigation events.
        /// </summary>
        private void InitializeObservers()
        {
            List<INavigable> observers = new List<INavigable>();

            observers.Add(LibraryViewModel.GetInstance());
            observers.Add(StoreViewModel.GetInstance());
            observers.Add(StreamConfigViewModel.GetInstance());
            observers.Add(TwitchLoginViewModel.GetInstance());

            this.Observers = observers;
        }
    }
    //// End class
}
//// End namespace