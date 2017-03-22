namespace Ana.Source.Scanners.InputCorrelator
{
    using ActionScheduler;
    using BackgroundScans.Prefilters;
    using Engine;
    using Engine.Input.HotKeys;
    using Engine.Input.Keyboard;
    using LabelThresholder;
    using Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UserSettings;
    using Utils.Extensions;

    internal class InputCorrelatorModel : ScannerBase, IObserver<KeyState>
    {
        private List<Hotkey> hotKeys;

        public InputCorrelatorModel(Action updateScanCount) : base(
            scannerName: "Input Correlator",
            isRepeated: true,
            dependencyBehavior: new DependencyBehavior(dependencies: typeof(ISnapshotPrefilter)))
        {
            this.UpdateScanCount = updateScanCount;
            this.ProgressLock = new Object();
            this.HotKeys = new List<Hotkey>();
        }

        public List<Hotkey> HotKeys
        {
            get
            {
                return this.hotKeys;
            }

            set
            {
                this.hotKeys = value;
            }
        }

        private Snapshot Snapshot { get; set; }

        private Action UpdateScanCount { get; set; }

        /// <summary>
        /// Gets or sets the time to consider a fired key event as active.
        /// </summary>
        private Int32 TimeOutIntervalMs { get; set; }

        private DateTime LastActivated { get; set; }

        private Object ProgressLock { get; set; }

        public void OnNext(KeyState value)
        {
            if (value.PressedKeys.IsNullOrEmpty())
            {
                return;
            }

            // If any of our keyboard hotkeys include the current set of pressed keys, trigger activation/deactivation
            if (this.HotKeys.Where(x => x.GetType().IsAssignableFrom(typeof(KeyboardHotkey))).Cast<KeyboardHotkey>().Any(x => x.GetActivationKeys().All(y => value.PressedKeys.Contains(y))))
            {
                this.LastActivated = DateTime.Now;
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        protected override void OnBegin()
        {
            this.InitializeObjects();

            // Initialize labeled snapshot
            this.Snapshot = SnapshotManager.GetInstance().GetActiveSnapshot(createIfNone: true).Clone(this.ScannerName);
            this.Snapshot.SetLabelType(typeof(Int16));

            if (this.Snapshot == null)
            {
                this.Cancel();
                return;
            }

            // Initialize with no correlation
            this.Snapshot.SetElementLabels<Int16>(0);
            this.TimeOutIntervalMs = SettingsViewModel.GetInstance().InputCorrelatorTimeOutInterval;

            base.OnBegin();
        }

        protected override void OnUpdate()
        {
            // Read memory to update previous and current values
            this.Snapshot.ReadAllMemory();

            Boolean conditionValid = this.IsInputConditionValid(this.Snapshot.GetTimeSinceLastUpdate());
            Int32 processedPages = 0;

            // Note the duplicated code here is an optimization to minimize comparisons done per iteration
            if (conditionValid)
            {
                Parallel.ForEach(
                this.Snapshot.Cast<SnapshotRegion>(),
                SettingsViewModel.GetInstance().ParallelSettings,
                (region) =>
                {
                    if (!region.CanCompare())
                    {
                        return;
                    }

                    foreach (SnapshotElementIterator element in region)
                    {
                        if (element.Changed())
                        {
                            ((dynamic)element).ElementLabel++;
                        }
                    }

                    lock (this.ProgressLock)
                    {
                        processedPages++;
                        this.UpdateProgress(processedPages, this.Snapshot.RegionCount);
                    }
                });
            }
            else
            {
                Parallel.ForEach(
                this.Snapshot.Cast<SnapshotRegion>(),
                SettingsViewModel.GetInstance().ParallelSettings,
                (region) =>
                {
                    if (!region.CanCompare())
                    {
                        return;
                    }

                    foreach (SnapshotElementIterator element in region)
                    {
                        if (element.Changed())
                        {
                            ((dynamic)element).ElementLabel--;
                        }
                    }

                    lock (this.ProgressLock)
                    {
                        processedPages++;
                        this.UpdateProgress(processedPages, this.Snapshot.RegionCount);
                    }
                });
            }

            this.UpdateScanCount?.Invoke();

            base.OnUpdate();
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            // Prefilter items with negative penalties (ie constantly changing variables)
            this.Snapshot.SetAllValidBits(false);

            foreach (SnapshotRegion region in this.Snapshot)
            {
                for (IEnumerator<SnapshotElementIterator> enumerator = region.IterateElements(PointerIncrementMode.LabelsOnly); enumerator.MoveNext();)
                {
                    SnapshotElementIterator element = enumerator.Current;

                    if ((Int16)element.ElementLabel > 0)
                    {
                        element.SetValid(true);
                    }
                }
            }

            this.Snapshot.DiscardInvalidRegions();

            SnapshotManager.GetInstance().SaveSnapshot(this.Snapshot);

            this.CleanUp();
            LabelThresholderViewModel.GetInstance().OpenLabelThresholder();

            base.OnEnd();
        }

        private void InitializeObjects()
        {
            this.LastActivated = DateTime.MinValue;
            this.InitializeObservers();
        }

        private void InitializeObservers()
        {
            EngineCore.GetInstance().Input?.GetKeyboardCapture().WeakSubscribe(this);
        }

        private Boolean IsInputConditionValid(DateTime updateTime)
        {
            if ((updateTime - this.LastActivated).TotalMilliseconds < this.TimeOutIntervalMs)
            {
                return true;
            }

            return false;
        }

        private void CleanUp()
        {
            this.Snapshot = null;

            EngineCore.GetInstance().Input?.GetKeyboardCapture().Unsubscribe(this);
        }
    }
    //// End class
}
//// End namespace