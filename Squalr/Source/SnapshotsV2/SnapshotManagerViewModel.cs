namespace Squalr.Source.SnapshotsV2
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Properties;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Snapshot Manager.
    /// </summary>
    internal class SnapshotManagerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="SnapshotManagerViewModel"/> class.
        /// </summary>
        private static Lazy<SnapshotManagerViewModel> snapshotManagerViewModelInstance = new Lazy<SnapshotManagerViewModel>(
                () => { return new SnapshotManagerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="SnapshotManagerViewModel"/> class from being created.
        /// </summary>
        private SnapshotManagerViewModel() : base("Snapshot Manager")
        {
            this.AccessLock = new Object();
            this.ObserverLock = new Object();
            this.Snapshots = new Stack<Snapshot>();
            this.DeletedSnapshots = new Stack<Snapshot>();
            this.SnapshotObservers = new List<ISnapshotObserver>();

            // Note: Not async to avoid updates slower than the perception threshold
            this.ClearSnapshotsCommand = new RelayCommand(() => this.ClearSnapshots(), () => true);
            this.UndoSnapshotCommand = new RelayCommand(() => this.UndoSnapshot(), () => true);
            this.RedoSnapshotCommand = new RelayCommand(() => this.RedoSnapshot(), () => true);

            Task.Run(() => DockingViewModel.GetInstance().RegisterViewModel(this));
        }

        /// <summary>
        /// Gets a command to start a new scan.
        /// </summary>
        public ICommand ClearSnapshotsCommand { get; private set; }

        /// <summary>
        /// Gets a command to undo the last scan.
        /// </summary>
        public ICommand UndoSnapshotCommand { get; private set; }

        /// <summary>
        /// Gets a command to redo the last scan.
        /// </summary>
        public ICommand RedoSnapshotCommand { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="SnapshotManagerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static SnapshotManagerViewModel GetInstance()
        {
            return snapshotManagerViewModelInstance.Value;
        }

        /// <summary>
        /// Recieves an update of the active snapshot.
        /// </summary>
        /// <param name="snapshot">The active snapshot.</param>
        public void Update(Snapshot snapshot)
        {
            this.RaisePropertyChanged(nameof(this.Snapshots));
            this.RaisePropertyChanged(nameof(this.DeletedSnapshots));
        }

        /// <summary>
        /// Gets the snapshots being managed.
        /// </summary>
        public Stack<Snapshot> Snapshots { get; private set; }

        /// <summary>
        /// Gets the deleted snapshots for the capability of redoing after undo.
        /// </summary>
        public Stack<Snapshot> DeletedSnapshots { get; private set; }

        /// <summary>
        /// Gets or sets a lock to ensure multiple entities do not try and update the snapshot list at the same time.
        /// </summary>
        private Object AccessLock { get; set; }

        /// <summary>
        /// Gets or sets a lock to ensure multiple entities do not try and update the snapshot list at the same time.
        /// </summary>
        private Object ObserverLock { get; set; }

        /// <summary>
        /// Gets or sets objects observing changes in the active snapshot.
        /// </summary>
        private List<ISnapshotObserver> SnapshotObservers { get; set; }

        /// <summary>
        /// The size limit for snapshots to be saved in the snapshot history (1GB).
        /// </summary>
        private const UInt64 SizeLimit = 1UL << 30;

        /// <summary>
        /// Subscribes the given object to changes in the active snapshot.
        /// </summary>
        /// <param name="snapshotObserver">The object to observe active snapshot changes.</param>
        public void Subscribe(ISnapshotObserver snapshotObserver)
        {
            lock (this.ObserverLock)
            {
                if (!this.SnapshotObservers.Contains(snapshotObserver))
                {
                    this.SnapshotObservers.Add(snapshotObserver);
                    snapshotObserver.Update(this.GetSnapshot(SnapshotRetrievalMode.FromActiveSnapshot));
                }
            }
        }

        /// <summary>
        /// Unsubscribes the given object from changes in the active snapshot.
        /// </summary>
        /// <param name="snapshotObserver">The object to observe active snapshot changes.</param>
        public void Unsubscribe(ISnapshotObserver snapshotObserver)
        {
            lock (this.ObserverLock)
            {
                if (this.SnapshotObservers.Contains(snapshotObserver))
                {
                    this.SnapshotObservers.Remove(snapshotObserver);
                }
            }
        }

        /// <summary>
        /// Gets a snapshot based on the provided mode. Will not read any memory.
        /// </summary>
        /// <param name="snapshotCreationMode">The method of snapshot retrieval.</param>
        /// <returns>The collected snapshot.</returns>
        public Snapshot GetSnapshot(SnapshotRetrievalMode snapshotCreationMode)
        {
            switch (snapshotCreationMode)
            {
                case SnapshotRetrievalMode.FromActiveSnapshot:
                    return this.GetActiveSnapshot();
                case SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter:
                    return this.GetActiveSnapshotCreateIfNone();
                case SnapshotRetrievalMode.FromSettings:
                    return this.CreateSnapshotFromSettings();
                case SnapshotRetrievalMode.FromUserModeMemory:
                    return this.CreateSnapshotFromUsermodeMemory();
                case SnapshotRetrievalMode.FromHeap:
                    throw new NotImplementedException();
                case SnapshotRetrievalMode.FromStack:
                    throw new NotImplementedException();
                default:
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unknown snapshot retrieval mode");
                    return null;
            }
        }

        /// <summary>
        /// Returns the memory regions associated with the current snapshot. If none exist, a query will be done. Will not read any memory.
        /// </summary>
        /// <returns>The current active snapshot of memory in the target process.</returns>
        private Snapshot GetActiveSnapshotCreateIfNone()
        {
            lock (this.AccessLock)
            {
                if (this.Snapshots.Count == 0 || this.Snapshots.Peek() == null || this.Snapshots.Peek().ElementCount == 0)
                {
                    // TODO: Reinstate prefilter
                    return this.CreateSnapshotFromUsermodeMemory();
                    // Snapshot snapshot = Prefilter.GetInstance().GetPrefilteredSnapshot();
                    // snapshot.UpdateSettings(ScanResultsViewModel.GetInstance().ActiveType, SettingsViewModel.GetInstance().Alignment);
                    // return snapshot;
                }

                // Return the snapshot
                return this.Snapshots.Peek();
            }
        }

        /// <summary>
        /// Returns the memory regions associated with the current snapshot. If none exist, a query will be done. Will not read any memory.
        /// </summary>
        /// <returns>The current active snapshot of memory in the target process.</returns>
        private Snapshot GetActiveSnapshot()
        {
            lock (this.AccessLock)
            {
                // Take a snapshot if there are none, or the current one is empty
                if (this.Snapshots.Count == 0 || this.Snapshots.Peek() == null || this.Snapshots.Peek().ElementCount == 0)
                {
                    return null;
                }

                // Return the snapshot
                return this.Snapshots.Peek();
            }
        }

        /// <summary>
        /// Creates a snapshot from all usermode memory. Will not read any memory.
        /// </summary>
        /// <returns>A snapshot created from usermode memory.</returns>
        private Snapshot CreateSnapshotFromUsermodeMemory()
        {
            MemoryProtectionEnum requiredPageFlags = 0;
            MemoryProtectionEnum excludedPageFlags = 0;
            MemoryTypeEnum allowedTypeFlags = MemoryTypeEnum.None | MemoryTypeEnum.Private | MemoryTypeEnum.Image | MemoryTypeEnum.Mapped;

            IntPtr startAddress = IntPtr.Zero;
            IntPtr endAddress = EngineCore.GetInstance().VirtualMemory.GetUserModeRegion().EndAddress;

            List<MemoryRegion> memoryRegions = new List<MemoryRegion>();
            IEnumerable<NormalizedRegion> virtualPages = EngineCore.GetInstance().VirtualMemory.GetVirtualPages(
                requiredPageFlags,
                excludedPageFlags,
                allowedTypeFlags,
                startAddress,
                endAddress);

            foreach (NormalizedRegion virtualPage in virtualPages)
            {
                memoryRegions.Add(new MemoryRegion(virtualPage.BaseAddress, virtualPage.RegionSize));
            }

            return new Snapshot(memoryRegions);
        }

        /// <summary>
        /// Creates a new snapshot of memory in the target process. Will not read any memory.
        /// </summary>
        /// <returns>The snapshot of memory taken in the target process.</returns>
        private Snapshot CreateSnapshotFromSettings()
        {
            MemoryProtectionEnum requiredPageFlags = SettingsViewModel.GetInstance().GetRequiredProtectionSettings();
            MemoryProtectionEnum excludedPageFlags = SettingsViewModel.GetInstance().GetExcludedProtectionSettings();
            MemoryTypeEnum allowedTypeFlags = SettingsViewModel.GetInstance().GetAllowedTypeSettings();

            IntPtr startAddress, endAddress;

            if (SettingsViewModel.GetInstance().IsUserMode)
            {
                startAddress = IntPtr.Zero;
                endAddress = EngineCore.GetInstance().VirtualMemory.GetUserModeRegion().EndAddress;
            }
            else
            {
                startAddress = SettingsViewModel.GetInstance().StartAddress.ToIntPtr();
                endAddress = SettingsViewModel.GetInstance().EndAddress.ToIntPtr();
            }

            List<MemoryRegion> memoryRegions = new List<MemoryRegion>();
            IEnumerable<NormalizedRegion> virtualPages = EngineCore.GetInstance().VirtualMemory.GetVirtualPages(
                requiredPageFlags,
                excludedPageFlags,
                allowedTypeFlags,
                startAddress,
                endAddress);

            // Convert each virtual page to a snapshot region
            foreach (NormalizedRegion virtualPage in virtualPages)
            {
                memoryRegions.Add(new MemoryRegion(virtualPage.BaseAddress, virtualPage.RegionSize));
            }

            return new Snapshot(memoryRegions);
        }

        /// <summary>
        /// Reverses an undo action.
        /// </summary>
        public void RedoSnapshot()
        {
            lock (this.AccessLock)
            {
                if (this.DeletedSnapshots.Count == 0)
                {
                    return;
                }

                this.Snapshots.Push(this.DeletedSnapshots.Pop());
                this.NotifyObservers();
            }
        }

        /// <summary>
        /// Undoes the current active snapshot, reverting to the previous snapshot.
        /// </summary>
        public void UndoSnapshot()
        {
            lock (this.AccessLock)
            {
                if (this.Snapshots.Count == 0)
                {
                    return;
                }

                this.DeletedSnapshots.Push(this.Snapshots.Pop());

                if (this.DeletedSnapshots.Peek() == null)
                {
                    this.DeletedSnapshots.Pop();
                }

                this.NotifyObservers();
            }
        }

        /// <summary>
        /// Clears all snapshot records.
        /// </summary>
        public void ClearSnapshots()
        {
            lock (this.AccessLock)
            {
                this.Snapshots.Clear();
                this.DeletedSnapshots.Clear();
                this.NotifyObservers();
            }
        }

        /// <summary>
        /// Saves a new snapshot, which will become the current active snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to save.</param>
        public void SaveSnapshot(Snapshot snapshot)
        {
            lock (this.AccessLock)
            {
                // Remove null snapshot if exists
                if (this.Snapshots.Count != 0 && this.Snapshots.Peek() == null)
                {
                    this.Snapshots.Pop();
                }

                // Do not keep large snapshots in the undo history
                if (this.Snapshots.Count != 0 && this.Snapshots.Peek() != null && this.Snapshots.Peek().ByteCount > SnapshotManagerViewModel.SizeLimit)
                {
                    this.Snapshots.Pop();
                }

                this.Snapshots.Push(snapshot);
                this.DeletedSnapshots.Clear();
                this.NotifyObservers();
            }
        }

        /// <summary>
        /// Notify all observing objects of an active snapshot change.
        /// </summary>
        private void NotifyObservers()
        {
            lock (this.ObserverLock)
            {
                Snapshot activeSnapshot = this.GetSnapshot(SnapshotRetrievalMode.FromActiveSnapshot);

                foreach (ISnapshotObserver observer in this.SnapshotObservers)
                {
                    observer.Update(activeSnapshot);
                }
            }
        }
    }
    //// End class
}
//// End namespace