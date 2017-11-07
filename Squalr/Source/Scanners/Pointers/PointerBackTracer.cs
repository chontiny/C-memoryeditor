namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.DataStructures;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to collect all pointers in the running process.
    /// </summary>
    internal class PointerBackTracer : ScheduledTask
    {
        /// <summary>
        /// Time in milliseconds between scans.
        /// </summary>
        private const Int32 RescanTime = 256;

        /// <summary>
        /// The rounding size for pointer destinations.
        /// </summary>
        private const Int32 PointerRadius = 1024;

        private const Int32 MaxAdd = 4096;

        /// <summary>
        /// Creates an instance of the <see cref="PointerBackTracer" /> class.
        /// </summary>
        public PointerBackTracer(UInt64 targetAddress, Action<IList<Snapshot>> levelSnapshotsCallback) : base(
            taskName: "Pointer Back Tracer",
            isRepeated: false,
            trackProgress: false)
        {
            this.ProgressLock = new Object();
            this.TargetAddress = targetAddress;

            this.MaxPointerLevel = 5;
            this.MaxPointerOffset = 2048;

            // TODO: Improvement: Make a snapshot from base modules to determine if an address is contained in O(log(n)) instead of O(n)

            this.Dependencies.Enqueue(new PointerCollector(this.SetSnapshot, this.SetPointerPool));
        }

        /// <summary>
        /// Gets or sets the current snapshot being parsed.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        private IDictionary<UInt64, UInt64> PointerPool { get; set; }

        private IEnumerable<NormalizedModule> BaseModules { get; set; }

        private UInt32 MaxPointerLevel { get; set; }

        private UInt64 MaxPointerOffset { get; set; }

        private UInt64 TargetAddress { get; set; }

        private List<Snapshot> LevelSnapshots { get; set; }

        private Object ProgressLock { get; set; }

        protected override void OnBegin()
        {
            this.UpdateInterval = PointerBackTracer.RescanTime;

            this.Snapshot = SnapshotManager.GetInstance().CreateSnapshotFromUsermodeMemory();
            this.LevelSnapshots = new List<Snapshot>();

            this.BaseModules = EngineCore.GetInstance().VirtualMemory.GetModules();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            UInt64 processedPointers = 0;
            Snapshot previousLevelSnapshot = new Snapshot();
            SnapshotRegion region = new SnapshotRegion(this.TargetAddress.ToIntPtr(), 1);

            region.Expand(PointerBackTracer.PointerRadius);
            previousLevelSnapshot.AddSnapshotRegions(region);

            // Add the address we are looking for as the base
            this.LevelSnapshots.Add(previousLevelSnapshot);

            for (Int32 level = 1; level <= this.MaxPointerLevel; level++)
            {
                ConcurrentHashSet<UInt64> currentLevelPointers = new ConcurrentHashSet<UInt64>();

                Parallel.ForEach(
                    this.PointerPool,
                    SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                    (pointer) =>
                {
                    // Ensure if this is a max level pointer that it is from an acceptable base address (ie static)
                    if (level == MaxPointerLevel && !BaseModules.Any(module => module.ContainsAddress(pointer.Key)))
                    {
                        return;
                    }

                    // Accept this pointer if it is points to the previous level snapshot
                    if (previousLevelSnapshot.ContainsAddress(pointer.Value))
                    {
                        // TODO: Potentially round pointers here?

                        currentLevelPointers.Add(pointer.Key);
                    }

                    lock (this.ProgressLock)
                    {
                        processedPointers++;

                        // Limit how often we update the progress
                        if (processedPointers % 1000 == 0)
                        {
                            this.UpdateProgress((processedPointers / this.MaxPointerLevel).ToInt32(), this.PointerPool.Count, canFinalize: false);
                        }
                    }
                });

                previousLevelSnapshot = new Snapshot();

                IList<SnapshotRegion> levelRegions = new List<SnapshotRegion>();

                foreach (UInt64 pointer in currentLevelPointers)
                {
                    region = new SnapshotRegion(pointer.ToIntPtr(), PointerBackTracer.PointerRadius);
                    region.Expand(PointerBackTracer.PointerRadius);
                    levelRegions.Add(region);
                }

                previousLevelSnapshot.AddSnapshotRegions(levelRegions);

                // Add the pointers for this level to the global accepted list
                this.LevelSnapshots.Add(previousLevelSnapshot);
            }
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.Snapshot = null;
            this.PointerPool = null;
            this.LevelSnapshots = null;

            this.UpdateProgress(ScheduledTask.MaximumProgress);
        }

        private void SetSnapshot(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
        }

        private void SetPointerPool(IDictionary<UInt64, UInt64> pointerPool)
        {
            this.PointerPool = pointerPool;
        }
    }
    //// End class
}
//// End namespace