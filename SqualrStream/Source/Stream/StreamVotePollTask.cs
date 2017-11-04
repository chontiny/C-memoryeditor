namespace SqualrStream.Source.Stream
{
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Threading;

    /// <summary>
    /// Task to poll for the current cheat votes.
    /// </summary>
    internal class StreamVotePollTask : ScheduledTask
    {
        /// <summary>
        /// The interval between refreshes.
        /// </summary>
        private const Int32 RefreshInterval = 5000;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamVotePollTask" /> class.
        /// </summary>
        public StreamVotePollTask(Action updateAction) : base(taskName: "Stream Vote Poll", isRepeated: true, trackProgress: false)
        {
            this.UpdateAction = updateAction;
            this.UpdateInterval = StreamVotePollTask.RefreshInterval;

            this.Start();
        }

        /// <summary>
        /// Gets or sets the refresh action.
        /// </summary>
        private Action UpdateAction { get; set; }

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
            this.UpdateAction();
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