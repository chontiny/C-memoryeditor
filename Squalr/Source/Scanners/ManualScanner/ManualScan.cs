namespace Squalr.Source.Scanners.ManualScanner
{
    using ScanConstraints;
    using Snapshots;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Output;
    using Squalr.Properties;
    using Squalr.Source.Scanners.ValueCollector;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A memory scanning class for classic manual memory scanning techniques.
    /// </summary>
    internal class ManualScan : ScheduledTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualScan" /> class.
        /// </summary>
        public ManualScan() : base(
            taskName: "Manual Scan",
            isRepeated: false,
            trackProgress: true)
        {
            this.Dependencies.Enqueue(new ValueCollectorModel(SnapshotManagerViewModel.SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter, callback: this.SetSnapshot));
        }

        /// <summary>
        /// Gets or sets the snapshot on which we perform the manual scan.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        /// <summary>
        /// Gets or sets the scan constraint manager.
        /// </summary>
        private ScanConstraintManager ScanConstraintManager { get; set; }

        /// <summary>
        /// Sets the scan constraints for this scan.
        /// </summary>
        /// <param name="scanConstraintManager">The scan constraint manager, which contains all scan constraints.</param>
        public void SetScanConstraintManager(ScanConstraintManager scanConstraintManager)
        {
            this.ScanConstraintManager = scanConstraintManager.Clone();
        }

        /// <summary>
        /// Called when the scan begins.
        /// </summary>
        protected override void OnBegin()
        {
            // Initialize snapshot
            this.Snapshot = this.Snapshot?.Clone(this.TaskName);

            if (this.Snapshot == null || this.ScanConstraintManager == null || this.ScanConstraintManager.Count() <= 0)
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
            cancellationToken.ThrowIfCancellationRequested();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Int32 processedPages = 0;
            Int32 regionCount = this.Snapshot.RegionCount;
            ConcurrentBag<IList<SnapshotRegion>> regions = new ConcurrentBag<IList<SnapshotRegion>>();

            Parallel.ForEach(
                this.Snapshot.OptimizedSnapshotRegions,
                SettingsViewModel.GetInstance().ParallelSettingsFastest,
                (region) =>
                {
                    // Perform comparisons
                    IList<SnapshotRegion> results = region.CompareAll(this.ScanConstraintManager);

                    if (!results.IsNullOrEmpty())
                    {
                        regions.Add(results);
                    }

                    // Update progress every N regions
                    if (Interlocked.Increment(ref processedPages) % 32 == 0)
                    {
                        // Check for canceled scan
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        this.UpdateProgress(processedPages, regionCount, canFinalize: false);
                    }
                });
            //// End foreach Region

            // Exit if canceled
            cancellationToken.ThrowIfCancellationRequested();

            this.Snapshot = new Snapshot(this.TaskName, regions.SelectMany(region => region));

            stopwatch.Stop();
            Output.Log(LogLevel.Info, "Scan complete in: " + stopwatch.Elapsed);
        }

        /// <summary>
        /// Called when the scan ends.
        /// </summary>
        protected override void OnEnd()
        {
            SnapshotManagerViewModel.GetInstance().SaveSnapshot(this.Snapshot);
            this.Snapshot = null;

            this.UpdateProgress(ScheduledTask.MaximumProgress);
        }

        /// <summary>
        /// Sets the snapshot to scan.
        /// </summary>
        /// <param name="snapshot">The snapshot to scan.</param>
        private void SetSnapshot(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
        }
    }
    //// End class
}
//// End namespace