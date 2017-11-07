namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Utils.DataStructures;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Class to collect all pointers in the running process.
    /// </summary>
    internal class PointerTracer : ScheduledTask
    {
        /// <summary>
        /// The rounding size for pointer destinations.
        /// </summary>
        private const Int32 ChunkSize = 1024;

        /// <summary>
        /// Gets or sets the number of regions processed by this prefilter.
        /// </summary>
        private Int64 processedCount;

        /// <summary>
        /// Creates an instance of the <see cref="PointerTracer" /> class.
        /// </summary>
        public PointerTracer() : base(
            taskName: "Pointer Tracer",
            isRepeated: false,
            trackProgress: false)
        {
            this.AccessLock = new Object();
        }

        /// <summary>
        /// Gets or sets the current snapshot being parsed.
        /// </summary>
        private Snapshot CurrentSnapshot { get; set; }

        private Object AccessLock { get; set; }

        private ConcurrentHashSet<UInt64> FoundPointerDestinations { get; set; }

        /// <summary>
        /// Gets all found pointers in the external process.
        /// </summary>
        /// <returns>A set of all found pointers.</returns>
        public IEnumerable<IntPtr> GetFoundPointerDestinations()
        {
            return null;
        }

        protected override void OnBegin()
        {
            this.CurrentSnapshot = SnapshotManager.GetInstance().CreateSnapshotFromUsermodeMemory();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            /*
            ConcurrentBag<SnapshotRegionDeprecating> previousLevelRegions = new ConcurrentBag<SnapshotRegionDeprecating>();
            previousLevelRegions.Add(this.AddressToRegion(this.TargetAddress));

            this.ConnectedPointers.Clear();
            this.SetAcceptedBases();

            // Add the address we are looking for as the base
            this.ConnectedPointers.Add(new ConcurrentDictionary<IntPtr, IntPtr>());
            this.ConnectedPointers.Last()[this.TargetAddress] = IntPtr.Zero;

            for (Int32 level = 1; level <= this.MaxPointerLevel; level++)
            {
                // Create snapshot from previous level regions to leverage the merging and sorting capabilities of a snapshot
                SnapshotDeprecating previousLevel = new SnapshotDeprecating<Null>(previousLevelRegions);
                ConcurrentDictionary<IntPtr, IntPtr> levelPointers = new ConcurrentDictionary<IntPtr, IntPtr>();

                Parallel.ForEach(
                    this.PointerPool,
                    SettingsViewModel.GetInstance().ParallelSettings,
                    (pointer) =>
                {
                    // Ensure if this is a max level pointer that it is from an acceptable base address (ie static)
                    if (level == MaxPointerLevel && !AcceptedBases.ContainsAddress(pointer.Key))
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

                previousLevelRegions = new ConcurrentBag<SnapshotRegionDeprecating>();

                // Construct new target region list from this level of pointers
                Parallel.ForEach(
                    levelPointers,
                    SettingsViewModel.GetInstance().ParallelSettings,
                    (pointer) =>
                {
                    previousLevelRegions.Add(AddressToRegion(pointer.Key));
                });
            }

            this.PointerPool.Clear();
            */
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
    //// End class
}
//// End namespace