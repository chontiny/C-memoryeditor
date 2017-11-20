namespace Squalr.Source.Scanners.ChangeCounter
{
    using LabelThresholder;
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine.Types;
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// Gets or sets the snapshot being labeled with change counts.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        private UInt16 MinChanges { get; set; }

        private UInt16 MaxChanges { get; set; }

        private Object ProgressLock { get; set; }

        private Action UpdateScanCount { get; set; }

        public void SetMinChanges(UInt16 minChanges)
        {
            this.MinChanges = minChanges;
        }

        public void SetMaxChanges(UInt16 maxChanges)
        {
            this.MaxChanges = maxChanges;
        }

        public void Stop()
        {
            this.EndUpdateLoop();
        }

        protected override void OnBegin()
        {
            // Initialize labeled snapshot
            this.Snapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter).Clone(this.TaskName);
            this.Snapshot.LabelDataType = DataTypes.UInt16;

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
                this.Snapshot.SnapshotRegions,
                SettingsViewModel.GetInstance().ParallelSettingsFast,
                (region) =>
            {
                if (!region.CanCompare())
                {
                    return;
                }

                for (IEnumerator<SnapshotRegionComparer> enumerator = region.IterateElements(); enumerator.MoveNext();)
                {
                    SnapshotRegionComparer element = enumerator.Current;

                    throw new NotImplementedException();
                    // Perform the comparison based on the current scan constraint
                    // if (element.Compare())
                    {
                        // element.ElementLabel = (UInt16)((UInt16)element.ElementLabel + 1);
                    }
                }

                // Update progress
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
            SnapshotManagerViewModel.GetInstance().SaveSnapshot(this.Snapshot);
            LabelThresholderViewModel.GetInstance().OpenLabelThresholder();
            this.Snapshot = null;
        }
    }
    //// End class
}
//// End namespace