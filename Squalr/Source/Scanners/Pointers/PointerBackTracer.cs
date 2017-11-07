namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Utils.DataStructures;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Class to collect all pointers in the running process.
    /// </summary>
    internal class PointerBackTracer : ScheduledTask
    {
        /// <summary>
        /// Time in milliseconds between scans.
        /// </summary>
        private const Int32 RescanTime = 256;

        /// <summary>
        /// The rounding size for pointer destinations.
        /// </summary>
        private const Int32 ChunkSize = 1024;

        /// <summary>
        /// Gets or sets the number of regions processed by this prefilter.
        /// </summary>
        private Int64 processedCount;

        /// <summary>
        /// Creates an instance of the <see cref="PointerBackTracer" /> class.
        /// </summary>
        public PointerBackTracer() : base(
            taskName: "Pointer Back Tracer",
            isRepeated: true,
            trackProgress: false)
        {
            this.AccessLock = new Object();

            this.Dependencies.Enqueue(new PointerCollector());
        }

        /// <summary>
        /// Gets or sets the current snapshot being parsed.
        /// </summary>
        private Snapshot CurrentSnapshot { get; set; }

        private Object AccessLock { get; set; }

        private ConcurrentHashSet<UInt64> FoundPointerDestinations { get; set; }

        /// <summary>
        /// Gets all found pointers in the external process.
        /// </summary>
        /// <returns>A set of all found pointers.</returns>
        public IEnumerable<IntPtr> GetFoundPointerDestinations()
        {
            return null;
        }

        protected override void OnBegin()
        {
            this.UpdateInterval = PointerBackTracer.RescanTime;

            this.CurrentSnapshot = SnapshotManager.GetInstance().CreateSnapshotFromUsermodeMemory();
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
        }
    }
    //// End class
}
//// End namespace