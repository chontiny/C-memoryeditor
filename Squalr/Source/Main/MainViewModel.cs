namespace Squalr.Source.Main
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.Logging;
    using Squalr.Engine.OS;
    using Squalr.Source.ChangeLog;
    using Squalr.Source.Docking;
    using Squalr.Source.Output;
    using Squirrel;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

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
            this.UpdateApp();

            Squalr.Engine.Projects.Compiler.Compile(true);

            // Attach our view model to the engine's output
            Logger.Subscribe(OutputViewModel.GetInstance());

            if (Vectors.HasVectorSupport)
            {
                Logger.Log(LogLevel.Info, "Hardware acceleration enabled");
                Logger.Log(LogLevel.Info, "Vector size: " + System.Numerics.Vector<Byte>.Count);
            }

            Logger.Log(LogLevel.Info, "Squalr started");

            this.DisplayChangeLogCommand = new RelayCommand(() => ChangeLogViewModel.GetInstance().DisplayChangeLog(new Content.ChangeLog().TransformText()), () => true);
        }

        /// <summary>
        /// Gets the command to open the change log.
        /// </summary>
        public ICommand DisplayChangeLogCommand { get; private set; }

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
            // SolutionExplorerViewModel.GetInstance().DisableAllProjectItems();

            base.Close(window);
        }

        private void UpdateApp()
        {
            Task.Run(async () =>
            {
                try
                {
                    using (UpdateManager manager = await UpdateManager.GitHubUpdateManager("https://github.com/Squalr/Squalr"))
                    {
                        UpdateInfo updates = await manager.CheckForUpdate();
                        ReleaseEntry lastVersion = updates?.ReleasesToApply?.OrderBy(x => x.Version).LastOrDefault();

                        if (lastVersion == null)
                        {
                            Logger.Log(LogLevel.Info, "Squalr is up to date.");
                            return;
                        }

                        Logger.Log(LogLevel.Info, "New version of Squalr found. Downloading files in background...");

                        await manager.DownloadReleases(new[] { lastVersion });
                        await manager.ApplyReleases(updates);
                        await manager.UpdateApp();

                        Logger.Log(LogLevel.Info, "New Squalr version downloaded. Restart the application to apply updates.");
                    }

                    UpdateManager.RestartApp();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error fetching Squalr updates.", ex);
                }
            });
        }
    }
    //// End class
}
//// End namesapce