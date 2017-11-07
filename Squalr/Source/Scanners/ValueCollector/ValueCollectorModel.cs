namespace Squalr.Source.Scanners.ValueCollector
{
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collect values for the current snapshot, or a new one if none exists. The values are then assigned to a new snapshot.
    /// </summary>
    internal class ValueCollectorModel : ScheduledTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCollectorModel" /> class.
        /// </summary>
        public ValueCollectorModel(Action<Snapshot> callback = null) : base(
            taskName: "Value Collector",
            isRepeated: false,
            trackProgress: true)
        {
            this.CallBack = callback;
            this.ProgressLock = new Object();
        }

        /// <summary>
        /// The callback to call with the collected snapshot. If none specified, the collected snapshot is set as the current results.
        /// </summary>
        private Action<Snapshot> CallBack;

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
            this.Snapshot = SnapshotManager.GetInstance().GetActiveSnapshot(createIfNone: true).Clone(this.TaskName);
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
                    // Check for canceled scan
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Read the memory for this region
                    region.ReadAllMemory(keepValues: true, readSuccess: out _);

                    // Update progress
                    lock (this.ProgressLock)
                    {
                        processedRegions++;

                        // Limit how often we update the progress
                        if (processedRegions % 10 == 0)
                        {
                            this.UpdateProgress(processedRegions, this.Snapshot.RegionCount, canFinalize: false);
                        }
                    }
                });

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Called when the scan ends.
        /// </summary>
        protected override void OnEnd()
        {
            if (this.CallBack == null)
            {
                SnapshotManager.GetInstance().SaveSnapshot(this.Snapshot);
            }
            else
            {
                this.CallBack?.Invoke(this.Snapshot);
            }

            this.Snapshot = null;

            this.UpdateProgress(ScheduledTask.MaximumProgress);
        }
    }
    //// End class
}
//// End namespace