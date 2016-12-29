namespace Ana.View
{
    using Chromium.Event;
    using Chromium.WebBrowser;
    using Controls;
    using Source.CheatBrowser;
    using Source.Project;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    /// <summary>
    /// Interaction logic for CheatBrowser.xaml
    /// </summary>
    internal partial class CheatBrowser : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// The file extension for cheat files
        /// </summary>
        private const String FileExtension = ".hax";

        /// <summary>
        /// The subdirectory to store downloaded cheat files
        /// </summary>
        private const String FileStorageDirectoryName = "Cheats";

        /// <summary>
        /// The filter to use for the save file dialog
        /// </summary>
        private const String ExtensionFilter = "Cheat File(*.Hax)|*.hax|All files(*.*)|*.*";

        /// <summary>
        /// The path to all user application files
        /// </summary>
        private static readonly String ApplicationFiles = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// The complete directory to store saved cheat files
        /// </summary>
        private static readonly String SavePath = Path.Combine(Path.Combine(ApplicationFiles, Assembly.GetExecutingAssembly().GetName().Name), CheatBrowser.FileStorageDirectoryName);

        private ChromiumWebBrowser browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatBrowser" /> class
        /// </summary>
        public CheatBrowser()
        {
            this.InitializeComponent();

            // Windows forms hosting -- TODO: Phase this out
            this.browser = new ChromiumWebBrowser();
            this.browserGrid.Children.Add(WinformsHostingHelper.CreateHostedControl(this.browser));

            this.browser.DownloadHandler.OnBeforeDownload += DownloadHandler_OnBeforeDownload;
            this.browser.DownloadHandler.OnDownloadUpdated += DownloadHandler_OnDownloadUpdated;

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

        private void DownloadHandler_OnBeforeDownload(Object sender, CfxOnBeforeDownloadEventArgs e)
        {
            e.Callback.Continue(Path.GetTempFileName(), false);
        }

        private void DownloadHandler_OnDownloadUpdated(Object sender, CfxOnDownloadUpdatedEventArgs e)
        {
            if (e.DownloadItem.IsComplete)
            {
                ProjectExplorerViewModel.GetInstance().ImportSpecificProjectCommand.Execute(e.DownloadItem.FullPath);
            }
        }

        private void MenuItemClick(Object sender, RoutedEventArgs e)
        {
            this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }
    }
    //// End class
}
//// End namespace