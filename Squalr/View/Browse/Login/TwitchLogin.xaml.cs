namespace Squalr.View
{
    using Source.Output;
    using Squalr.Source.Analytics;
    using Squalr.Source.Browse.TwitchLogin;
    using Squalr.Source.Utils;
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using System.Windows;
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for TwitchLogin.xaml.
    /// </summary>
    internal partial class TwitchLogin : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitchLogin" /> class.
        /// </summary>
        public TwitchLogin()
        {
            this.InitializeComponent();

            this.browser.Navigating += this.BrowserNavigating;
            this.TwitchLoginViewModel.NavigateHomeCommand.Execute(this.browser);

            // This ensures that we will actually hit the login screen without a saved login
            WebBrowserHelper.ClearCache();
        }


        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public TwitchLoginViewModel TwitchLoginViewModel
        {
            get
            {
                return this.DataContext as TwitchLoginViewModel;
            }
        }

        /// <summary>
        /// Invoked when the browser is about to navigate to a new page.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Cancel event args.</param>
        private void BrowserNavigating(Object sender, NavigatingCancelEventArgs e)
        {
            if (e == null || e.Uri == null || e.Uri.AbsoluteUri == null)
            {
                return;
            }

            try
            {
                if (e.Uri.AbsoluteUri.StartsWith(TwitchLoginViewModel.LoginCallbackUrl))
                {
                    NameValueCollection queries = HttpUtility.ParseQueryString(e.Uri.Query);
                    String code = queries["code"];
                    e.Cancel = true;

                    // Pass the code from the query string to the view model command to fetch the oauth codes
                    this.TwitchLoginViewModel.PerformLoginCommand.Execute(code);

                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error handling web request", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.TwitchLogin, ex);
            }
        }

        /// <summary>
        /// Invoked when the cancel button is clicked. Closes the window.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void CancelButtonClick(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
    //// End class
}
//// End namespace