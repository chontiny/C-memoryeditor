namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
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
        private const Int32 ChunkSize = 1024;

        private const Int32 MaxAdd = 4096;

        /// <summary>
        /// Creates an instance of the <see cref="PointerBackTracer" /> class.
        /// </summary>
        public PointerBackTracer(UInt64 targetAddress) : base(
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

        private Int32 MaxPointerLevel { get; set; }

        private UInt64 MaxPointerOffset { get; set; }

        private UInt64 TargetAddress { get; set; }

        private List<ConcurrentDictionary<UInt64, UInt64>> ConnectedPointers { get; set; }

        private Object ProgressLock { get; set; }

        protected override void OnBegin()
        {
            this.UpdateInterval = PointerBackTracer.RescanTime;

            this.Snapshot = SnapshotManager.GetInstance().CreateSnapshotFromUsermodeMemory();
            this.ConnectedPointers = new List<ConcurrentDictionary<UInt64, UInt64>>();

            this.BaseModules = EngineCore.GetInstance().VirtualMemory.GetModules();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            ConcurrentBag<SnapshotRegion> previousLevelRegions = new ConcurrentBag<SnapshotRegion>();
            previousLevelRegions.Add(this.AddressToRegion(this.TargetAddress));

            this.ConnectedPointers.Clear();
            this.SetAcceptedBases();

            // Add the address we are looking for as the base
            this.ConnectedPointers.Add(new ConcurrentDictionary<UInt64, UInt64>());
            this.ConnectedPointers.Last()[this.TargetAddress] = 0;

            for (Int32 level = 1; level <= this.MaxPointerLevel; level++)
            {
                // Create snapshot from previous level regions to leverage the merging and sorting capabilities of a snapshot
                Snapshot previousLevel = new Snapshot(previousLevelRegions);
                ConcurrentDictionary<UInt64, UInt64> levelPointers = new ConcurrentDictionary<UInt64, UInt64>();

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
                    if (previousLevel.ContainsAddress(pointer.Value))
                    {
                        levelPointers[pointer.Key] = pointer.Value;
                    }
                });

                // Add the pointers for this level to the global accepted list
                this.ConnectedPointers.Add(levelPointers);

                previousLevelRegions = new ConcurrentBag<SnapshotRegion>();

                // Construct new target region list from this level of pointers
                Parallel.ForEach(
                    levelPointers,
                    SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                    (pointer) =>
                {
                    previousLevelRegions.Add(AddressToRegion(pointer.Key));
                });
            }

            this.PointerPool.Clear();

        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
        }

        private void SetAcceptedBases()
        {
            /*
            this.PrintDebugTag();

            IEnumerable<NormalizedModule> modules = EngineCore.GetInstance().OperatingSystemAdapter.GetModules();
            List<SnapshotRegionDeprecating> acceptedBaseRegions = new List<SnapshotRegionDeprecating>();

            // Gather regions from every module as valid base addresses
            modules.ForEach(x => acceptedBaseRegions.Add(new SnapshotRegionDeprecating<Null>(new NormalizedRegion(x.BaseAddress, x.RegionSize))));

            // Convert regions into a snapshot
            this.AcceptedBases = new SnapshotDeprecating<Null>(acceptedBaseRegions);
            */
        }

        private SnapshotRegion AddressToRegion(UInt64 address)
        {
            return new SnapshotRegion(new NormalizedRegion(address.ToIntPtr().Subtract(this.MaxPointerOffset), this.MaxPointerOffset * 2));
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