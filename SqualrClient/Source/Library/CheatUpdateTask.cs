namespace SqualrClient.Source.Stream
{
    using SqualrCore.Source.ActionScheduler;
    using System;

    /// <summary>
    /// Task to update cheats.
    /// </summary>
    internal class CheatUpdateTask : ScheduledTask
    {
        /// <summary>
        /// The interval in milliseconds between refreshes.
        /// </summary>
        private const Int32 RefreshInterval = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatUpdateTask" /> class.
        /// </summary>
        public CheatUpdateTask(Action updateAction) : base(taskName: "Cheat Updater", isRepeated: true, trackProgress: false)
        {
            this.UpdateAction = updateAction;
            this.UpdateInterval = CheatUpdateTask.RefreshInterval;

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