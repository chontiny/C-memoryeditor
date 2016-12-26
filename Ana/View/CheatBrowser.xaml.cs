namespace Ana.View
{
    using Controls;
    using Gecko;
    using Source.CheatBrowser;
    using Source.Engine;
    using Source.Project;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CheatBrowser.xaml
    /// </summary>
    internal partial class CheatBrowser : UserControl
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

        protected GeckoWebBrowser browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatBrowser" /> class
        /// </summary>
        public CheatBrowser()
        {
            this.InitializeComponent();

            // Initialize engine for Gecko Fx web browser
            if (EngineCore.GetInstance().OperatingSystemAdapter.IsAnathena32Bit())
            {
                Xpcom.Initialize(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Libraries/xulrunner-32"));
            }
            else if (EngineCore.GetInstance().OperatingSystemAdapter.IsAnathena64Bit())
            {
                Xpcom.Initialize(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Libraries/xulrunner-64"));
            }

            // Windows forms hosting -- TODO: Phase this out
            this.browser = new GeckoWebBrowser();
            this.browserGrid.Children.Add(WinformsHostingHelper.CreateHostedControl(this.browser));
            LauncherDialog.Download += this.LauncherDialogDownload;

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
        /// Handle save file dialog for downloads. Code partly ripped from the internet, no idea how it works.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void LauncherDialogDownload(Object sender, LauncherDialogEvent e)
        {
            nsILocalFile objectTarget = Xpcom.CreateInstance<nsILocalFile>("@mozilla.org/file/local;1");
            String fileName = Path.Combine(CheatBrowser.SavePath, e.Filename);

            using (nsAString nsaString = new nsAString(@Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\temp.tmp"))
            {
                objectTarget.InitWithPath(nsaString);
            }

            using (Stream saveStream = File.OpenWrite(fileName))
            {
                nsIURI source = IOService.CreateNsIUri(e.Url);
                nsIURI destination = IOService.CreateNsIUri(new Uri(fileName).AbsoluteUri);
                nsAStringBase stringBase = new nsAString(Path.GetFileName(fileName));

                nsIWebBrowserPersist persist = Xpcom.CreateInstance<nsIWebBrowserPersist>("@mozilla.org/embedding/browser/nsWebBrowserPersist;1");
                nsIDownloadManager downloadMan = null;
                downloadMan = Xpcom.CreateInstance<nsIDownloadManager>("@mozilla.org/download-manager;1");
                nsIDownload download = downloadMan.AddDownload(0, source, destination, stringBase, e.Mime, 0, null, persist, false);

                if (download != null)
                {
                    persist.SetPersistFlagsAttribute(2 | 32 | 16384);
                    persist.SetProgressListenerAttribute(download);
                    persist.SaveURI(source, null, null, null, null, (nsISupports)destination, null);
                }

                saveStream.Flush();
                saveStream.Close();

                // Import the downloaded file into the current project
                ProjectExplorerViewModel.GetInstance().ImportSpecificProjectCommand.Execute(fileName);
            }
        }

        private void MenuItemClick(Object sender, System.Windows.RoutedEventArgs e)
        {
            this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }
    }
    //// End class
}
//// End namespace