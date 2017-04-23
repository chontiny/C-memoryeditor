namespace Ana.Source.StreamWeaver
{
    using ActionScheduler;
    using System;

    /// <summary>
    /// Task to update the Stream Weaver.
    /// </summary>
    internal class StreamWeaverTask : ScheduledTask
    {
        /// <summary>
        /// The interval between refreshes.
        /// </summary>
        private const Int32 RefreshInterval = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamWeaverTask" /> class.
        /// </summary>
        public StreamWeaverTask(Action updateAction) : base(taskName: "Stream Weaver", isRepeated: true, trackProgress: false)
        {
            this.UpdateAction = updateAction;
            this.UpdateInterval = StreamWeaverTask.RefreshInterval;

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