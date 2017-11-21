namespace Squalr.Source.Scanners.ManualScanner
{
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A scanner to discard invalid regions
    /// </summary>
    internal class FinalizerScan : ScheduledTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinalizerScan" /> class.
        /// </summary>
        public FinalizerScan(Snapshot snapshot) : base(
            taskName: "Finalizer",
            isRepeated: false,
            trackProgress: true)
        {
            this.Snapshot = snapshot;
        }

        /// <summary>
        /// Gets or sets the snapshot on which we perform the finalization.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        /// <summary>
        /// Called when the scan begins.
        /// </summary>
        protected override void OnBegin()
        {
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Int32 processedPages = 0;
            ConcurrentBag<SnapshotRegion> validRegions = new ConcurrentBag<SnapshotRegion>();

            // Find the regions with valid bits set
            Parallel.ForEach(
                this.Snapshot.OptimizedSnapshotRegions,
                SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                (region) =>
                {
                    // Valid bits no longer exist, so the finalizer is pretty useless?

                    // Update progress every N regions
                    if (Interlocked.Increment(ref processedPages) % 32 == 0)
                    {
                        // Check for canceled scan
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        this.UpdateProgress(processedPages, this.Snapshot.RegionCount, canFinalize: false);
                    }
                });
            //// End foreach Region

            // Exit if canceled
            cancellationToken.ThrowIfCancellationRequested();

            this.Snapshot = new Snapshot(this.Snapshot.SnapshotName, validRegions);

            this.UpdateProgress(ScheduledTask.MaximumProgress);
        }

        /// <summary>
        /// Called when the scan ends.
        /// </summary>
        protected override void OnEnd()
        {
            SnapshotManagerViewModel.GetInstance().SaveSnapshot(this.Snapshot);
            Snapshot = null;
        }
    }
    //// End class
}
//// End namespace