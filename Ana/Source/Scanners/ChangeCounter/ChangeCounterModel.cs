namespace Ana.Source.Scanners.ChangeCounter
{
    using LabelThresholder;
    using Snapshots;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UserSettings;

    internal class ChangeCounterModel : ScannerBase
    {
        public ChangeCounterModel(Action updateScanCount) : base("Change Counter")
        {
            this.UpdateScanCount = updateScanCount;
            this.ProgressLock = new Object();
        }

        /// <summary>
        /// Gets or sets the snapshot being labeled with change counts
        /// </summary>
        private ISnapshot Snapshot { get; set; }

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

        public override void Begin()
        {
            // Initialize labeled snapshot
            this.Snapshot = ((dynamic)SnapshotManager.GetInstance().GetActiveSnapshot()).CloneAs<Int32, UInt16>();

            if (this.Snapshot == null)
            {
                return;
            }

            // Initialize change counts to zero
           ((dynamic)this.Snapshot).SetElementLabels(0);

            base.Begin();
        }

        protected override void OnUpdate()
        {
            Int32 processedPages = 0;

            // Read memory to get current values
            this.Snapshot.ReadAllMemory();

            Parallel.ForEach(
                this.Snapshot.Cast<Object>(),
                SettingsViewModel.GetInstance().ParallelSettings,
                (Object regionObject) =>
            {
                ISnapshotRegion<Int32, UInt16> region = regionObject as ISnapshotRegion<Int32, UInt16>;

                if (!region.CanCompare())
                {
                    return;
                }

                foreach (ISnapshotElementRef<Int32, UInt16> element in region)
                {
                    if (element.Changed())
                    {
                        ((dynamic)element).ElementLabel++;
                    }
                }

                lock (ProgressLock)
                {
                    processedPages++;
                }
            });

            base.OnUpdate();
            this.UpdateScanCount?.Invoke();
        }

        /// <summary>
        /// Called when the repeated task completes
        /// </summary>
        protected override void OnEnd()
        {
            base.OnEnd();
            // this.Snapshot.ScanMethod = this.ScannerName;

            SnapshotManager.GetInstance().SaveSnapshot(this.Snapshot);
            LabelThresholderViewModel.GetInstance().IsVisible = true;
            LabelThresholderViewModel.GetInstance().IsActive = true;
        }

        private void CleanUp()
        {
            this.Snapshot = null;
        }
    }
    //// End class
}
//// End namespace