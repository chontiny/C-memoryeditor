namespace Ana.View
{
    using Source.CheatBrowser;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatBrowser" /> class
        /// </summary>
        public CheatBrowser()
        {
            this.InitializeComponent();

            this.browser.Focus();

            // Windows forms hosting -- TODO: Phase this out
            // this.browser = new GeckoWebBrowser();
            // this.browserGrid.Children.Add(WinformsHostingHelper.CreateHostedControl(this.browser));

            // this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
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

        private void MenuItemClick(Object sender, RoutedEventArgs e)
        {
            // this.CheatBrowserViewModel.NavigateHomeCommand.Execute(this.browser);
        }
    }
    //// End class
}
//// End namespace