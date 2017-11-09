namespace Squalr.Source.Scanners.ChangeCounter
{
    using LabelThresholder;
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ChangeCounterModel : ScheduledTask
    {
        /// <summary>
        /// The number of scans completed.
        /// </summary>
        private Int32 scanCount;

        public ChangeCounterModel(Action updateScanCount) : base(
            taskName: "Change Counter",
            isRepeated: true,
            trackProgress: true)
        {
            this.UpdateScanCount = updateScanCount;
            this.ProgressLock = new Object();
        }

        /// <summary>
        /// Gets or sets the snapshot being labeled with change counts.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        private UInt16 MinChanges { get; set; }

        private UInt16 MaxChanges { get; set; }

        private Object ProgressLock { get; set; }

        private Action UpdateScanCount { get; set; }

        /// <summary>
        /// Gets the number of scans that have been executed.
        /// </summary>
        public Int32 ScanCount
        {
            get
            {
                return this.scanCount;
            }

            private set
            {
                this.scanCount = value;
                this.RaisePropertyChanged(nameof(this.ScanCount));
            }
        }

        public void SetMinChanges(UInt16 minChanges)
        {
            this.MinChanges = minChanges;
        }

        public void SetMaxChanges(UInt16 maxChanges)
        {
            this.MaxChanges = maxChanges;
        }

        protected override void OnBegin()
        {
            // Initialize labeled snapshot
            this.Snapshot = SnapshotManager.GetInstance().GetActiveSnapshot(createIfNone: true).Clone(this.TaskName);
            this.Snapshot.SetLabelType(typeof(UInt16));

            if (this.Snapshot == null)
            {
                return;
            }

            // Initialize change counts to zero
            this.Snapshot.SetElementLabels<UInt16>(0);

            this.ScanCount = 0;
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Int32 processedPages = 0;

            // Read memory to get current values
            this.Snapshot.ReadAllMemory();

            Parallel.ForEach(
                this.Snapshot.Cast<SnapshotRegion>(),
                SettingsViewModel.GetInstance().ParallelSettingsFast,
                (region) =>
            {
                if (!region.CanCompare())
                {
                    return;
                }

                foreach (SnapshotElementIterator element in region)
                {
                    element.ElementLabel = (UInt16)((UInt16)element.ElementLabel + 1);
                }

                lock (this.ProgressLock)
                {
                    processedPages++;
                    this.UpdateProgress(processedPages, this.Snapshot.RegionCount, canFinalize: false);
                }
            });

            this.ScanCount++;

            this.UpdateScanCount?.Invoke();
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            SnapshotManager.GetInstance().SaveSnapshot(this.Snapshot);
            LabelThresholderViewModel.GetInstance().OpenLabelThresholder();
            this.Snapshot = null;
        }
    }
    //// End class
}
//// End namespace