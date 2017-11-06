namespace Squalr.Source.Snapshots
{
    using Results.ScanResults;
    using Squalr.Properties;
    using Squalr.Source.Prefilters;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.OperatingSystems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Manages snapshots of memory taken from the target process.
    /// </summary>
    internal class SnapshotManager : IScanResultsObserver
    {
        /// <summary>
        /// Singleton instance of Snapshot Manager.
        /// </summary>
        private static Lazy<SnapshotManager> snapshotManagerInstance = new Lazy<SnapshotManager>(
            () => { return new SnapshotManager(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="SnapshotManager" /> class from being created.
        /// </summary>
        private SnapshotManager()
        {
            this.AccessLock = new Object();
            this.ObserverLock = new Object();
            this.Snapshots = new Stack<Snapshot>();
            this.DeletedSnapshots = new Stack<Snapshot>();
            this.SnapshotObservers = new List<ISnapshotObserver>();
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
        /// Gets a singleton instance of the <see cref="SnapshotManager"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static SnapshotManager GetInstance()
        {
            return SnapshotManager.snapshotManagerInstance.Value;
        }

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
                    snapshotObserver.Update(this.GetActiveSnapshot(createIfNone: false));
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
        /// Returns the memory regions associated with the current snapshot. If none exist, a query will be done.
        /// </summary>
        /// <param name="createIfNone">Creates a snapshot if none exists.</param>
        /// <returns>The current active snapshot of memory in the target process.</returns>
        public Snapshot GetActiveSnapshot(Boolean createIfNone = true)
        {
            lock (this.AccessLock)
            {
                // Take a snapshot if there are none, or the current one is empty
                if (this.Snapshots.Count == 0 || this.Snapshots.Peek() == null || this.Snapshots.Peek().ElementCount == 0)
                {
                    if (createIfNone)
                    {
                        return Prefilter.GetInstance().GetPrefilteredSnapshot();
                    }
                    else
                    {
                        return null;
                    }
                }

                // Return the snapshot
                return this.Snapshots.Peek();
            }
        }

        public Snapshot CreateSnapshotFromUsermodeMemory()
        {
            MemoryProtectionEnum requiredPageFlags = 0;
            MemoryProtectionEnum excludedPageFlags = 0;
            MemoryTypeEnum allowedTypeFlags = MemoryTypeEnum.None | MemoryTypeEnum.Private | MemoryTypeEnum.Image | MemoryTypeEnum.Mapped;

            IntPtr startAddress = IntPtr.Zero;
            IntPtr endAddress = EngineCore.GetInstance().OperatingSystem.GetUserModeRegion().EndAddress;

            List<SnapshotRegion> memoryRegions = new List<SnapshotRegion>();
            IEnumerable<NormalizedRegion> virtualPages = EngineCore.GetInstance().OperatingSystem.GetVirtualPages(
                requiredPageFlags,
                excludedPageFlags,
                allowedTypeFlags,
                startAddress,
                endAddress);

            foreach (NormalizedRegion virtualPage in virtualPages)
            {
                memoryRegions.Add(new SnapshotRegion(virtualPage.BaseAddress, virtualPage.RegionSize));
            }

            return new Snapshot(memoryRegions);
        }

        /// <summary>
        /// Creates a new snapshot of memory in the target process. Will not read any memory.
        /// </summary>
        /// <returns>The snapshot of memory taken in the target process.</returns>
        public Snapshot CreateSnapshotFromSettings()
        {
            MemoryProtectionEnum requiredPageFlags = SettingsViewModel.GetInstance().GetRequiredProtectionSettings();
            MemoryProtectionEnum excludedPageFlags = SettingsViewModel.GetInstance().GetExcludedProtectionSettings();
            MemoryTypeEnum allowedTypeFlags = SettingsViewModel.GetInstance().GetAllowedTypeSettings();

            IntPtr startAddress, endAddress;

            if (SettingsViewModel.GetInstance().IsUserMode)
            {
                startAddress = IntPtr.Zero;
                endAddress = EngineCore.GetInstance().OperatingSystem.GetUserModeRegion().EndAddress;
            }
            else
            {
                startAddress = SettingsViewModel.GetInstance().StartAddress.ToIntPtr();
                endAddress = SettingsViewModel.GetInstance().EndAddress.ToIntPtr();
            }

            List<SnapshotRegion> memoryRegions = new List<SnapshotRegion>();
            IEnumerable<NormalizedRegion> virtualPages = EngineCore.GetInstance().OperatingSystem.GetVirtualPages(
                requiredPageFlags,
                excludedPageFlags,
                allowedTypeFlags,
                startAddress,
                endAddress);

            // Convert each virtual page to a snapshot region (a more condensed representation of the information)
            foreach (NormalizedRegion virtualPage in virtualPages)
            {
                memoryRegions.Add(new SnapshotRegion(virtualPage.BaseAddress, virtualPage.RegionSize));
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
        /// Updates the active type.
        /// </summary>
        /// <param name="activeType">The new active type.</param>
        public void Update(Type activeType)
        {
            this.GetActiveSnapshot(createIfNone: false)?.PropagateSettings();
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
                if (this.Snapshots.Count != 0 && this.Snapshots.Peek() != null && this.Snapshots.Peek().ByteCount > SnapshotManager.SizeLimit)
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
                Snapshot activeSnapshot = this.GetActiveSnapshot(createIfNone: false);
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