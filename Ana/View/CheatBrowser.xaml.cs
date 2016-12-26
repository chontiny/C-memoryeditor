namespace Ana.View
{
    using Controls;
    using Gecko;
    using Microsoft.Win32;
    using Source.CheatBrowser;
    using Source.Engine;
    using Source.Project;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Navigation;

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

            this.browser = new GeckoWebBrowser();

            this.browserGrid.Children.Add(WinformsHostingHelper.CreateHostedControl(this.browser));

            // LauncherDialog.Download += this.LauncherDialogDownload;

            // this.browser.Navigating += this.BrowserNavigating;
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
        /// Invoked when the browser is about to navigate to a new page
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Cancel event args</param>
        private void BrowserNavigating(Object sender, NavigatingCancelEventArgs e)
        {
            if (e == null || e.Uri == null || e.Uri.AbsoluteUri == null)
            {
                return;
            }

            try
            {
                if (e.Uri.Query.Split('&').Any(x => x.ToLower().EndsWith(FileExtension)))
                {
                    // Get filename from the url
                    String fileName = e.Uri.Query.Split('&').Where(x => x.ToLower().EndsWith(FileExtension)).First().Split('=').Last() ?? ("default" + FileExtension);

                    e.Cancel = true;

                    WebClient client = new WebClient();
                    client.DownloadDataCompleted += (source, args) => this.DownloadDataCompleted(source, args, fileName);
                    client.DownloadDataAsync(e.Uri);
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Handle save file dialog for downloads. Code ripped from the internet, no idea how it works
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event parameters</param>
        private void LauncherDialogDownload(Object sender, LauncherDialogEvent e)
        {
            nsILocalFile objectTarget = Xpcom.CreateInstance<nsILocalFile>("@mozilla.org/file/local;1");
            Stream saveStream;

            String fileName;
            String fileDirectory;

            using (nsAString nsaString = new nsAString(@Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\temp.tmp"))
            {
                objectTarget.InitWithPath(nsaString);
            }

            // Allow user to select the save location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ExtensionFilter;
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = e.Filename;

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            fileDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
            fileName = Path.GetFileName(saveFileDialog.FileName);

            if ((saveStream = File.OpenWrite(Path.Combine(fileDirectory, fileName))) == null)
            {
                return;
            }

            nsIURI source = IOService.CreateNsIUri(e.Url);
            nsIURI destination = IOService.CreateNsIUri(new Uri(Path.Combine(fileDirectory, fileName)).AbsoluteUri);
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
        }

        /// <summary>
        /// Invoked when a download is complete, before the bytes are written
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Download event args</param>
        /// <param name="fileName">The name of the file being downloaded</param>
        private void DownloadDataCompleted(Object sender, DownloadDataCompletedEventArgs e, String fileName)
        {
            try
            {
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }

                String saveLocation = Path.Combine(SavePath, fileName);
                File.WriteAllBytes(saveLocation, e.Result);

                ProjectExplorerViewModel.GetInstance().ImportSpecificProjectCommand.Execute(saveLocation);
            }
            catch
            {
                return;
            }
        }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }
    }
    //// End class
}
//// End namespace