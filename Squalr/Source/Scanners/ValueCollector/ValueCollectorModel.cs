namespace Squalr.Source.Scanners.ValueCollector
{
    using Snapshots;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Output;
    using Squalr.Properties;
    using System;
    using System.Diagnostics;
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
        public ValueCollectorModel(
            SnapshotManagerViewModel.SnapshotRetrievalMode snapshotRetrievalMode = SnapshotManagerViewModel.SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter,
            Snapshot defaultSnapshot = null,
            Action<Snapshot> callback = null) : base(
            taskName: "Value Collector",
            isRepeated: false,
            trackProgress: true)
        {
            this.SnapshotRetrievalMode = snapshotRetrievalMode;
            this.CallBack = callback;
        }

        /// <summary>
        /// Gets or sets the default snapshot to use instead of the snapshot retrieval mode.
        /// </summary>
        private Snapshot DefaultSnapshot { get; set; }

        /// <summary>
        /// Gets or sets the callback to call with the collected snapshot. If none specified, the collected snapshot is set as the current results.
        /// </summary>
        private Action<Snapshot> CallBack { get; set; }

        /// <summary>
        /// Gets or sets the method of snapshot retrieval.
        /// </summary>
        private SnapshotManagerViewModel.SnapshotRetrievalMode SnapshotRetrievalMode { get; set; }

        /// <summary>
        /// Gets or sets the snapshot on which we perform the value collection.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        /// <summary>
        /// Performs the value collection scan.
        /// </summary>
        protected override void OnBegin()
        {
            if (this.DefaultSnapshot != null)
            {
                // Use the provided default snapshot
                this.Snapshot = DefaultSnapshot;
            }
            else
            {
                // Otherwise retrieve it using the settings
                this.Snapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(this.SnapshotRetrievalMode)?.Clone(this.TaskName);
            }
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

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Read memory to get current values for each region
            Parallel.ForEach(
                this.Snapshot.OptimizedReadGroups,
                SettingsViewModel.GetInstance().ParallelSettingsFastest,
                (readGroup) =>
                {
                    // Read the memory for this region
                    readGroup.ReadAllMemory();

                    // Update progress every N regions
                    if (Interlocked.Increment(ref processedRegions) % 32 == 0)
                    {
                        // Check for canceled scan
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        this.UpdateProgress(processedRegions, this.Snapshot.RegionCount, canFinalize: false);
                    }
                });

            stopwatch.Stop();
            Output.Log(LogLevel.Info, "Values collected in: " + stopwatch.Elapsed);

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Called when the scan ends.
        /// </summary>
        protected override void OnEnd()
        {
            if (this.CallBack == null)
            {
                SnapshotManagerViewModel.GetInstance().SaveSnapshot(this.Snapshot);
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