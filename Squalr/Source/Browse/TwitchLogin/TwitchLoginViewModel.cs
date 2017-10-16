namespace Squalr.Source.Browse.TwitchLogin
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using Squalr.Source.Output;
    using Squalr.View.Browse.Login;
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Twitch Login.
    /// </summary>
    internal class TwitchLoginViewModel : ToolViewModel, INavigable
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(TwitchLoginViewModel);

        /// <summary>
        /// The url for the twitch login.
        /// </summary>
        public const String LoginUrl = "https://www.squalr.com/client/login";

        /// <summary>
        /// The callback url for the twitch login.
        /// </summary>
        public const String LoginCallbackUrl = "https://squalr.com/login/callback";

        /// <summary>
        /// Singleton instance of the <see cref="TwitchLoginViewModel" /> class.
        /// </summary>
        private static Lazy<TwitchLoginViewModel> twitchLoginViewModelInstance = new Lazy<TwitchLoginViewModel>(
                () => { return new TwitchLoginViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="TwitchLoginViewModel" /> class from being created.
        /// </summary>
        private TwitchLoginViewModel() : base("Twitch Login")
        {
            this.ContentId = TwitchLoginViewModel.ToolContentId;

            // Note: Cannot be async, navigation must take place on the same thread as GUI
            this.DisplayTwitchLoginCommand = new RelayCommand(() => this.DisplayTwitchLogin(), () => true);
            this.NavigateHomeCommand = new RelayCommand<WebBrowser>((browser) => this.NavigateHome(browser), (browser) => true);
            this.PerformLoginCommand = new RelayCommand<String>((code) => this.PerformLogin(code), (code) => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to navigate home.
        /// </summary>
        public ICommand NavigateHomeCommand { get; private set; }

        /// <summary>
        /// Gets the command to navigate home.
        /// </summary>
        public ICommand PerformLoginCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the twitch login screen.
        /// </summary>
        public ICommand DisplayTwitchLoginCommand { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="TwitchLoginViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static TwitchLoginViewModel GetInstance()
        {
            return twitchLoginViewModelInstance.Value;
        }

        /// <summary>
        /// Event fired when the browse view navigates to a new page.
        /// </summary>
        /// <param name="browsePage">The new browse page.</param>
        public void OnNavigate(BrowsePage browsePage)
        {
            switch (browsePage)
            {
                case BrowsePage.Login:
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Navigates home in the browser.
        /// </summary>
        /// <param name="browser">The web browser.</param>
        private void NavigateHome(WebBrowser browser)
        {
            browser.Navigate(TwitchLoginViewModel.LoginUrl);
        }

        /// <summary>
        /// Gets the twitch oauth access tokens using the provided code.
        /// </summary>
        /// <param name="code">The one time use exchange code to receive the access tokens.</param>
        private void PerformLogin(String code)
        {
            try
            {
                AccessTokens accessTokens = SqualrApi.GetAccessTokens(code);
                SqualrApi.Connect(accessTokens.AccessToken);
                SettingsViewModel.GetInstance().AccessTokens = accessTokens;

                User user = SqualrApi.GetTwitchUser(accessTokens.AccessToken);
                BrowseViewModel.GetInstance().ActiveUser = user;

                BrowseViewModel.GetInstance().IsLoggedIn = true;
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Error authorizing Twitch", ex);
            }
        }

        /// <summary>
        /// Displays the twitch login screen.
        /// </summary>
        private void DisplayTwitchLogin()
        {
            TwitchLoginPage twitchLoginScreen = new TwitchLoginPage();
            twitchLoginScreen.Owner = Application.Current.MainWindow;
            twitchLoginScreen.ShowDialog();
        }
    }
    //// End class
}
//// End namespace