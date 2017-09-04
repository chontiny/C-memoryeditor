namespace Squalr.Source.TwitchConfig
{
    using ActionScheduler;
    using System;

    /// <summary>
    /// Task to poll for the current cheat votes.
    /// </summary>
    internal class TwitchVotePollTask : ScheduledTask
    {
        /// <summary>
        /// The interval between refreshes.
        /// </summary>
        private const Int32 RefreshInterval = 3000;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitchVotePollTask" /> class.
        /// </summary>
        public TwitchVotePollTask(Action updateAction) : base(taskName: "Twitch Vote Poll", isRepeated: true, trackProgress: false)
        {
            this.UpdateAction = updateAction;
            this.UpdateInterval = TwitchVotePollTask.RefreshInterval;

            this.Begin();
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