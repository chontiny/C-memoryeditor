namespace Squalr.Source.Main
{
    using Squalr.Properties;
    using Squalr.Source.ProjectExplorer;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Output;
    using System;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Main view model.
    /// </summary>
    internal class MainViewModel : WindowHostViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="MainViewModel" /> class
        /// </summary>
        private static Lazy<MainViewModel> mainViewModelInstance = new Lazy<MainViewModel>(
                () => { return new MainViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="MainViewModel" /> class from being created.
        /// </summary>
        private MainViewModel() : base()
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Squalr developer tools started");
        }

        /// <summary>
        /// Default layout file for browsing cheats.
        /// </summary>
        protected override String DefaultLayoutResource { get { return "DefaultLayout.xml"; } }

        /// <summary>
        /// The save file for the docking layout.
        /// </summary>
        protected override String LayoutSaveFile { get { return "Layout.xml"; } }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        /// <returns>The singleton instance of the <see cref="MainViewModel" /> class.</returns>
        public static MainViewModel GetInstance()
        {
            return mainViewModelInstance.Value;
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        /// <param name="window">The window to close.</param>
        protected override void Close(Window window)
        {
            if (!ProjectExplorerViewModel.GetInstance().ProjectItemStorage.PromptSave())
            {
                return;
            }

            SettingsViewModel.GetInstance().Save();
            ProjectExplorerViewModel.GetInstance().DisableAllProjectItems();

            base.Close(window);
        }
    }
    //// End class
}
//// End namesapce