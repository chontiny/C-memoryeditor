namespace Squalr.Source.Scanners.ValueCollector
{
    using Snapshots;
    using Squalr.Properties;
    using Squalr.Source.Prefilters;
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collect values for the current snapshot, or a new one if none exists. The values are then assigned to a new snapshot.
    /// </summary>
    internal class ValueCollectorModel : ScannerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCollectorModel" /> class.
        /// </summary>
        public ValueCollectorModel() : base(
            scannerName: "Value Collector",
            isRepeated: false,
            dependencyBehavior: new DependencyBehavior(dependencies: typeof(ISnapshotPrefilter)))
        {
            this.ProgressLock = new Object();
        }

        /// <summary>
        /// Gets or sets the snapshot on which we perform the value collection.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        /// <summary>
        /// Gets or sets a lock object for updating scan progress.
        /// </summary>
        private Object ProgressLock { get; set; }

        /// <summary>
        /// Performs the value collection scan.
        /// </summary>
        protected override void OnBegin()
        {
            this.Snapshot = SnapshotManager.GetInstance().GetActiveSnapshot(createIfNone: true).Clone(this.ScannerName);

            base.OnBegin();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            if (this.Snapshot == null)
            {
                return;
            }

            Int32 processedRegions = 0;

            // Read memory to get current values for each region
            Parallel.ForEach(
                this.Snapshot.Cast<SnapshotRegion>(),
                SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                (region) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    region.ReadAllRegionMemory(keepValues: true, readSuccess: out _);

                    lock (this.ProgressLock)
                    {
                        processedRegions++;
                        this.UpdateProgress(processedRegions, this.Snapshot.RegionCount, canFinalize: false);
                    }
                });

            cancellationToken.ThrowIfCancellationRequested();

            this.UpdateProgress(ScheduledTask.MaximumProgress);

            base.OnUpdate(cancellationToken);
        }

        /// <summary>
        /// Called when the scan ends.
        /// </summary>
        protected override void OnEnd()
        {
            SnapshotManager.GetInstance().SaveSnapshot(this.Snapshot);

            this.Snapshot = null;

            base.OnEnd();
        }
    }
    //// End class
}
//// End namespace