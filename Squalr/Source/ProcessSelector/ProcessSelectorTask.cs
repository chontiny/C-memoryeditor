namespace Squalr.Source.ProcessSelector
{
    using ActionScheduler;
    using System;

    /// <summary>
    /// Task for the Process Selector.
    /// </summary>
    internal class ProcessSelectorTask : ScheduledTask
    {
        /// <summary>
        /// The interval between refreshes.
        /// </summary>
        private const Int32 RefreshInterval = 5000;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessSelectorTask" /> class.
        /// </summary>
        public ProcessSelectorTask(Action refreshAction) : base(taskName: "Process Selector", isRepeated: true, trackProgress: false)
        {
            this.RefreshAction = refreshAction;
            this.UpdateInterval = ProcessSelectorTask.RefreshInterval;

            this.Begin();
        }

        /// <summary>
        /// Gets or sets the refresh action.
        /// </summary>
        private Action RefreshAction { get; set; }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        protected override void OnUpdate()
        {
            this.RefreshAction();
            base.OnUpdate();
        }
    }
    //// End class
}
//// End namespace