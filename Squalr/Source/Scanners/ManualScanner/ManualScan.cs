namespace Squalr.Source.Scanners.ManualScanner
{
    using ScanConstraints;
    using Snapshots;
    using Squalr.Properties;
    using Squalr.Source.Scanners.ValueCollector;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
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
            this.ProgressLock = new Object();

            this.Dependencies.Enqueue(new ValueCollectorModel(SnapshotManagerViewModel.SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter, this.SetSnapshot));
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
        /// Gets or sets a lock object for updating scan progress.
        /// </summary>
        private Object ProgressLock { get; set; }

        /// <summary>
        /// Sets the scan constraints for this scan.
        /// </summary>
        /// <param name="scanConstraintManager">The scan constraint manager, which contains all scan constraints.</param>
        public void SetScanConstraintManager(ScanConstraintManager scanConstraintManager)
        {
            this.ScanConstraintManager = scanConstraintManager;
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
            Int32 processedPages = 0;
            Boolean hasRelativeConstraint = this.ScanConstraintManager.HasRelativeConstraint();

            cancellationToken.ThrowIfCancellationRequested();

            UInt64 sanity = 0;

            // Enforce each value constraint
            foreach (ScanConstraint scanConstraint in this.ScanConstraintManager)
            {
                this.Snapshot.SetAllValidBits(false);

                Parallel.ForEach(
                    this.Snapshot.SnapshotRegions,
                    SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                    (region) =>
                    {
                        // Check for canceled scan
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        // Ignore region if it requires current & previous values, but we cannot find them
                        // if (hasRelativeConstraint && !region.CanCompare())
                        {
                            //   return;
                        }

                        for (IEnumerator<SnapshotRegionComparer> enumerator = region.IterateElements(scanConstraint.Constraint, scanConstraint.ConstraintValue);
                            enumerator.MoveNext();)
                        {
                            SnapshotRegionComparer element = enumerator.Current;

                            var debug = element.Compare();

                            SByte[] result = new SByte[region.RegionSize];
                            Vector.AsVectorSByte(debug).CopyTo(result);

                            sanity += EngineCore.GetInstance().Architecture.GetVectorSize().ToUInt64();

                            // Perform the comparison based on the current scan constraint
                            // if (element.Compare())
                            {
                                // element.SetValid(true);
                            }
                        }
                        //// End foreach Element

                        lock (this.ProgressLock)
                        {
                            processedPages++;

                            // Limit how often we update the progress
                            if (processedPages % 10 == 0)
                            {
                                this.UpdateProgress(processedPages / this.ScanConstraintManager.Count(), this.Snapshot.RegionCount, canFinalize: false);
                            }
                        }
                    });
                //// End foreach Region

                // Exit if canceled
                cancellationToken.ThrowIfCancellationRequested();
            }
            //// End foreach Constraint
        }

        /// <summary>
        /// Called when the scan ends.
        /// </summary>
        protected override void OnEnd()
        {
            FinalizerScan finalizer = new FinalizerScan(this.Snapshot);
            finalizer.Start();

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