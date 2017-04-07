namespace Ana.View
{
    using Source.CheatBrowser;
    using Source.Output;
    using Source.Project;
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Windows;
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for CheatBrowser.xaml.
    /// </summary>
    internal partial class CheatBrowser : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// The file extension for cheat files.
        /// </summary>
        private const String FileExtension = ".hax";

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatBrowser" /> class.
        /// </summary>
        public CheatBrowser()
        {
            this.InitializeComponent();

            this.browser.Navigating += this.BrowserNavigating;
            this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public CheatBrowserViewModel CheatBrowserViewModel
        {
            get
            {
                return this.DataContext as CheatBrowserViewModel;
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
                NameValueCollection queries = HttpUtility.ParseQueryString(e.Uri.Query);
                String cheatName = queries["cheatName"];
                String native = queries["native"];

                // Handle file downloads
                if (cheatName != null && cheatName.EndsWith(CheatBrowser.FileExtension))
                {
                    e.Cancel = true;

                    try
                    {
                        // Load and import the file.
                        String file = Path.GetTempFileName();
                        URLDownloadToFile(null, e.Uri.AbsoluteUri, file, 0, IntPtr.Zero);
                        ProjectExplorerViewModel.GetInstance().ImportSpecificProjectCommand.Execute(file);
                    }
                    catch (Exception ex)
                    {
                        OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, ex.ToString());
                    }

                    return;
                }

                // Handle request to open the native browser
                if (native != null)
                {
                    Process.Start(new Uri(new Uri(CheatBrowserViewModel.HomeUrl), native).AbsoluteUri);
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, ex.ToString());
            }
        }

        /// <summary>
        /// Event for the navigate home menu item click.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void NavigateHomeMenuClick(Object sender, RoutedEventArgs e)
        {
            this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Int32 URLDownloadToFile([MarshalAs(UnmanagedType.IUnknown)] Object callerPointer, [MarshalAs(UnmanagedType.LPWStr)] String url,
            [MarshalAs(UnmanagedType.LPWStr)] String filePathWithName, Int32 reserved, IntPtr callBack);
    }
    //// End class
}
//// End namespace