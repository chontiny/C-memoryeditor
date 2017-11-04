namespace SqualrCore.Source.ProcessSelector
{
    using ActionScheduler;
    using System;
    using System.Threading;

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

            this.Start();
        }

        /// <summary>
        /// Gets or sets the refresh action.
        /// </summary>
        private Action RefreshAction { get; set; }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            this.RefreshAction();
            base.OnUpdate(cancellationToken);
        }
    }
    //// End class
}
//// End namespace