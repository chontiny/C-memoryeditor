namespace SqualrStream.View.Login
{
    using SqualrCore.Source.Analytics;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.Browser;
    using SqualrStream.Source.Browse.TwitchLogin;
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for TwitchLoginPage.xaml.
    /// </summary>
    internal partial class TwitchLoginPage : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitchLoginPage" /> class.
        /// </summary>
        public TwitchLoginPage()
        {
            this.InitializeComponent();

            this.browser.Navigating += this.BrowserNavigating;
            this.TwitchLoginViewModel.NavigateHomeCommand.Execute(this.browser);

            // This ensures that we will actually hit the login screen without a saved login
            WebBrowserHelper.ClearCache();
            this.HideScriptErrors();
        }

        /// <summary>
        /// Hides javascript warnings in the web browser control.
        /// </summary>
        public void HideScriptErrors()
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            Object objComWebBrowser = fiComWebBrowser?.GetValue(this.browser);

            objComWebBrowser?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new Object[] { true });
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
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.StreamClient, ex);
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