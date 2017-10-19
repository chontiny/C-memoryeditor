namespace SqualrCore.Source.Browse.StreamConfig
{
    using ActionScheduler;
    using System;

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
        public StreamVotePollTask(Action updateAction) : base(taskName: "Twitch Vote Poll", isRepeated: true, trackProgress: false)
        {
            this.UpdateAction = updateAction;
            this.UpdateInterval = StreamVotePollTask.RefreshInterval;

            this.Schedule();
        }

        /// <summary>
        /// Gets or sets the refresh action.
        /// </summary>
        private Action UpdateAction { get; set; }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        protected override void OnUpdate()
        {
            this.UpdateAction();
            base.OnUpdate();
        }
    }
    //// End class
}
//// End namespace