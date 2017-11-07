namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Class to collect all pointers in the running process.
    /// </summary>
    internal class PointerRetracer : ScheduledTask
    {
        /// <summary>
        /// Creates an instance of the <see cref="PointerRetracer" /> class.
        /// </summary>
        public PointerRetracer(UInt64 targetAddress) : base(
            taskName: "Pointer Retracer",
            isRepeated: false,
            trackProgress: true)
        {
            this.AccessLock = new Object();

            this.Dependencies.Enqueue(new PointerBackTracer(targetAddress, this.SetLevelSnapshots));
        }

        private IList<Snapshot> LevelSnapshots { get; set; }

        private Object AccessLock { get; set; }

        protected override void OnBegin()
        {
            if (this.LevelSnapshots == null)
            {
                this.Cancel();
            }
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.LevelSnapshots = null;
        }

        private void SetLevelSnapshots(IList<Snapshot> levelSnapshots)
        {
            this.LevelSnapshots = levelSnapshots;
        }
    }
    //// End class
}
//// End namespace