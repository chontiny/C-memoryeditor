namespace Squalr.Source.Scanners.ChangeCounter
{
    using ActionScheduler;
    using BackgroundScans.Prefilters;
    using LabelThresholder;
    using Snapshots;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UserSettings;

    internal class ChangeCounterModel : ScannerBase
    {
        public ChangeCounterModel(Action updateScanCount) : base(
            scannerName: "Change Counter",
            isRepeated: true,
            dependencyBehavior: new DependencyBehavior(dependencies: typeof(ISnapshotPrefilter)))
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
            this.Snapshot = SnapshotManager.GetInstance().GetActiveSnapshot(createIfNone: true).Clone(this.ScannerName);
            this.Snapshot.SetLabelType(typeof(UInt16));

            if (this.Snapshot == null)
            {
                return;
            }

            // Initialize change counts to zero
            this.Snapshot.SetElementLabels<UInt16>(0);

            base.OnBegin();
        }

        protected unsafe override void OnUpdate()
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
                    element.ElementLabel = (UInt16)element.ElementLabel + 1;
                }

                lock (this.ProgressLock)
                {
                    processedPages++;
                    this.UpdateProgress(processedPages, this.Snapshot.RegionCount);
                }
            });

            this.UpdateScanCount?.Invoke();

            base.OnUpdate();
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            SnapshotManager.GetInstance().SaveSnapshot(this.Snapshot);
            LabelThresholderViewModel.GetInstance().OpenLabelThresholder();
            this.Snapshot = null;

            base.OnEnd();
        }
    }
    //// End class
}
//// End namespace