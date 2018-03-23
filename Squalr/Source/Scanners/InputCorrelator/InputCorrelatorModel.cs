namespace Squalr.Source.Scanners.InputCorrelator
{
    using LabelThresholder;
    using Squalr.Engine;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Input.HotKeys;
    using Squalr.Engine.Input.Keyboard;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Properties;
    using Squalr.Source.Snapshots;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class InputCorrelatorModel : ScheduledTask, IObserver<KeyState>
    {
        private FullyObservableCollection<Hotkey> hotKeys;

        /// <summary>
        /// The number of scans completed.
        /// </summary>
        private Int32 scanCount;

        public InputCorrelatorModel(Action updateScanCount) : base(
            taskName: "Input Correlator",
            isRepeated: true,
            trackProgress: true)
        {
            this.UpdateScanCount = updateScanCount;
            this.ProgressLock = new Object();
            this.HotKeys = new FullyObservableCollection<Hotkey>();
        }

        public FullyObservableCollection<Hotkey> HotKeys
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
            this.Snapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter).Clone(this.TaskName);
            this.Snapshot.LabelDataType = DataType.Int16;

            if (this.Snapshot == null)
            {
                this.Cancel();
                return;
            }

            // Initialize with no correlation
            this.Snapshot.SetElementLabels<Int16>(0);
            this.TimeOutIntervalMs = SettingsViewModel.GetInstance().InputCorrelatorTimeOutInterval;
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            // Read memory to update previous and current values
            this.Snapshot.ReadAllMemory();

            Boolean conditionValid = this.IsInputConditionValid(this.Snapshot.GetTimeSinceLastUpdate());
            Int32 processedPages = 0;

            // Note the duplicated code here is an optimization to minimize comparisons done per iteration
            if (conditionValid)
            {
                Parallel.ForEach(
                this.Snapshot.SnapshotRegions,
                SettingsViewModel.GetInstance().ParallelSettingsFast,
                (region) =>
                {
                    if (!region.ReadGroup.CanCompare(hasRelativeConstraint: true))
                    {
                        return;
                    }

                    IEnumerator<SnapshotElementComparer> enumerator = region.IterateComparer(SnapshotElementComparer.PointerIncrementMode.AllPointers, null);

                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Compare();

                        ((dynamic)enumerator).ElementLabel++;
                    }

                    lock (this.ProgressLock)
                    {
                        processedPages++;
                        this.UpdateProgress(processedPages, this.Snapshot.RegionCount, canFinalize: false);
                    }
                });
            }
            else
            {
                Parallel.ForEach(
                this.Snapshot.SnapshotRegions,
                SettingsViewModel.GetInstance().ParallelSettingsFast,
                (region) =>
                {
                    if (!region.ReadGroup.CanCompare(hasRelativeConstraint: true))
                    {
                        return;
                    }

                    IEnumerator<SnapshotElementComparer> enumerator = region.IterateComparer(SnapshotElementComparer.PointerIncrementMode.AllPointers, null);

                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Compare();

                        ((dynamic)enumerator).ElementLabel--;
                    }

                    lock (this.ProgressLock)
                    {
                        processedPages++;
                        this.UpdateProgress(processedPages, this.Snapshot.RegionCount, canFinalize: false);
                    }
                });
            }

            this.UpdateScanCount?.Invoke();
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            // Prefilter items with negative penalties (ie constantly changing variables)
            //// this.Snapshot.SetAllValidBits(false);

            foreach (SnapshotRegion region in this.Snapshot.SnapshotRegions)
            {
                for (IEnumerator<SnapshotElementComparer> enumerator = region.IterateComparer(SnapshotElementComparer.PointerIncrementMode.ValuesOnly, null); enumerator.MoveNext();)
                {
                    SnapshotElementComparer element = enumerator.Current;

                    if ((Int16)element.ElementLabel > 0)
                    {
                        //// element.SetValid(true);
                    }
                }
            }

            ////  this.Snapshot.DiscardInvalidRegions();

            SnapshotManagerViewModel.GetInstance().SaveSnapshot(this.Snapshot);

            this.CleanUp();
            LabelThresholderViewModel.GetInstance().OpenLabelThresholder();
        }

        private void InitializeObjects()
        {
            this.LastActivated = DateTime.MinValue;
            this.InitializeObservers();
        }

        private void InitializeObservers()
        {
            Eng.GetInstance().Input?.GetKeyboardCapture().WeakSubscribe(this);
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

            Eng.GetInstance().Input?.GetKeyboardCapture().Unsubscribe(this);
        }
    }
    //// End class
}
//// End namespace