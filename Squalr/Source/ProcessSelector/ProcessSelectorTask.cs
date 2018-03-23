namespace Squalr.Source.ProcessSelector
{
    using Squalr.Engine.TaskScheduler;
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
        /// <param name="refreshAction">The callback process refresh list action.</param>
        public ProcessSelectorTask(Action refreshAction) : base(taskName: "Process Selector", isRepeated: true, trackProgress: false)
        {
            this.RefreshAction = refreshAction;
            this.UpdateInterval = ProcessSelectorTask.RefreshInterval;

            this.Start();
        }

        /// <summary>
        /// Gets or sets the callback process list refresh action.
        /// </summary>
        private Action RefreshAction { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
        }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            this.RefreshAction();
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
    //// End class
}
//// End namespace