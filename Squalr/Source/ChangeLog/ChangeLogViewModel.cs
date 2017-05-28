namespace Squalr.Source.ChangeLog
{
    using Mvvm;
    using System;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// View model for the Change Log.
    /// </summary>
    internal class ChangeLogViewModel : ViewModelBase
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
            this.changeLog = new Squalr.Content.ChangeLog().TransformText();
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
    }
    //// End class
}
//// End namespace