namespace SqualrCore.Source.ChangeLog
{
    using GalaSoft.MvvmLight;
    using SqualrCore.Source.Output;
    using System;
    using System.Deployment.Application;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// View model for the Change Log.
    /// </summary>
    public class ChangeLogViewModel : ViewModelBase
    {
        /// <summary>
        /// The changelog text.
        /// </summary>
        private String changeLog;

        /// <summary>
        /// Singleton instance of the <see cref="ChangeLogViewModel"/> class.
        /// </summary>
        private static Lazy<ChangeLogViewModel> changeLogViewModelInstance = new Lazy<ChangeLogViewModel>(
                () => { return new ChangeLogViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="ChangeLogViewModel" /> class.
        /// </summary>
        private ChangeLogViewModel()
        {
            this.changeLog = new Content.ChangeLog().TransformText();
        }

        /// <summary>
        /// Gets the changelog text.
        /// </summary>
        public String ChangeLog
        {
            get
            {
                return this.changeLog;
            }
        }

        /// <summary>
        /// Gets the title, including version, of the changelog.
        /// </summary>
        public String Title
        {
            get
            {
                return "Change Log - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ChangeLogViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ChangeLogViewModel GetInstance()
        {
            return ChangeLogViewModel.changeLogViewModelInstance.Value;
        }

        /// <summary>
        /// Displays the change log to the user if there has been a recent update.
        /// </summary>
        public void DisplayChangeLog()
        {
            try
            {
                if (!ApplicationDeployment.IsNetworkDeployed || !ApplicationDeployment.CurrentDeployment.IsFirstRun)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Error displaying change log", ex);
                return;
            }

            View.ChangeLog changeLog = new View.ChangeLog();
            changeLog.Owner = Application.Current.MainWindow;
            changeLog.ShowDialog();
        }
    }
    //// End class
}
//// End namespace