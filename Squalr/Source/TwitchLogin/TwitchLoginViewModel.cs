namespace Squalr.Source.CheatBrowser
{
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using Squalr.Source.Mvvm.Command;
    using System;
    using System.Threading;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Twitch Login.
    /// </summary>
    internal class TwitchLoginViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(TwitchLoginViewModel);

        /// <summary>
        /// The url for the twitch login.
        /// </summary>
        public const String LoginUrl = "https://www.squalr.com/Client/Login";

        /// <summary>
        /// The callback url for the twitch login.
        /// </summary>
        public const String LoginCallbackUrl = "https://squalr.com/Login/Callback";

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
            this.NavigateHomeCommand = new RelayCommand<WebBrowser>((browser) => this.NavigateHome(browser), (browser) => true);
            this.GetTwitchAccessTokensCommand = new RelayCommand<String>((code) => this.GetTwitchAccessTokens(code), (code) => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to navigate home.
        /// </summary>
        public ICommand NavigateHomeCommand { get; private set; }

        /// <summary>
        /// Gets the command to navigate home.
        /// </summary>
        public ICommand GetTwitchAccessTokensCommand { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="TwitchLoginViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static TwitchLoginViewModel GetInstance()
        {
            return twitchLoginViewModelInstance.Value;
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
        private void GetTwitchAccessTokens(String code)
        {
            TwitchTokens tokens = SqualrApi.GetTwitchTokens(code);
        }
    }
    //// End class
}
//// End namespace