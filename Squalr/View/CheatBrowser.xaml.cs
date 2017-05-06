namespace Squalr.View
{
    using Source.CheatBrowser;
    using Source.Output;
    using Source.ProjectExplorer;
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
        /// The URLMON library contains this function, URLDownloadToFile, which is a way to download files without user prompts. The ExecWB( _SAVEAS ) function always
        /// prompts the user, even if _DONTPROMPTUSER parameter is specified, for "internet security reasons". This function gets around those reasons.
        /// </summary>
        /// <param name="callerPointer">Pointer to caller object (AX).</param>
        /// <param name="url">String of the URL.</param>
        /// <param name="filePathWithName">String of the destination filename/path.</param>
        /// <param name="reserved">Reserved parameter.</param>
        /// <param name="callBack">A callback function to monitor progress or abort.</param>
        /// <returns>0 for okay.</returns>
        [DllImport("urlmon.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Int32 URLDownloadToFile(
            [MarshalAs(UnmanagedType.IUnknown)] Object callerPointer,
            [MarshalAs(UnmanagedType.LPWStr)] String url,
            [MarshalAs(UnmanagedType.LPWStr)] String filePathWithName,
            Int32 reserved,
            IntPtr callBack);

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
                        OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Error downloading project file", ex.ToString());
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
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error handling web request", ex.ToString());
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
    }
    //// End class
}
//// End namespace