namespace Ana.Source.ChangeLog
{
    using Mvvm;
    using System;
    using System.Reflection;

    /// <summary>
    /// View model for the Change Log
    /// </summary>
    internal class ChangeLogViewModel : ViewModelBase
    {
        private String changeLog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeLogViewModel" /> class.
        /// </summary>
        public ChangeLogViewModel()
        {
            this.changeLog = new Ana.Content.ChangeLog().TransformText();
        }

        public String ChangeLog
        {
            get
            {
                return this.changeLog;
            }
        }

        public String Title
        {
            get
            {
                return "Change Log - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
    }
    //// End class
}
//// End namespace