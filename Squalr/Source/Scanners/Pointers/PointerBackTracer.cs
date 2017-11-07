namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using Squalr.Properties;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements an algorithm which starts from the pointer destination, and works backwards finding each level of possible pointers.
    /// </summary>
    internal class PointerBackTracer : ScheduledTask
    {
        /// <summary>
        /// Time in milliseconds between scans.
        /// </summary>
        private const Int32 RescanTime = 256;

        /// <summary>
        /// Creates an instance of the <see cref="PointerBackTracer" /> class.
        /// </summary>
        public PointerBackTracer(
                UInt64 targetAddress,
                Action<Stack<ConcurrentDictionary<UInt64, UInt64>>> levelPointersCallback) : base(
            taskName: "Pointer Back Tracer",
            isRepeated: false,
            trackProgress: true)
        {
            this.ProgressLock = new Object();
            this.TargetAddress = targetAddress;

            this.PointerDepth = 3;
            this.PointerRadius = 1024;
            this.LevelPointersCallback = levelPointersCallback;

            this.Dependencies.Enqueue(new PointerCollector(this.SetModulePointers, this.SetHeapPointers));
        }

        private IDictionary<UInt64, UInt64> ModulePointers { get; set; }

        private IDictionary<UInt64, UInt64> HeapPointers { get; set; }

        private UInt32 PointerDepth { get; set; }

        private UInt64 PointerRadius { get; set; }

        private UInt64 TargetAddress { get; set; }

        private Stack<ConcurrentDictionary<UInt64, UInt64>> LevelPointers { get; set; }

        private Action<Stack<ConcurrentDictionary<UInt64, UInt64>>> LevelPointersCallback { get; set; }

        private Object ProgressLock { get; set; }

        protected override void OnBegin()
        {
            this.UpdateInterval = PointerBackTracer.RescanTime;

            this.LevelPointers = new Stack<ConcurrentDictionary<UInt64, UInt64>>();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            UInt64 processedPointers = 0;

            // Create a snapshot only containing the destination
            Snapshot destinationSnapshot = new Snapshot();
            SnapshotRegion destinationRegion = new SnapshotRegion(this.TargetAddress.ToIntPtr(), 1);
            destinationRegion.Expand(this.PointerRadius);
            destinationSnapshot.AddSnapshotRegions(destinationRegion);

            Snapshot previousLevelSnapshot = destinationSnapshot;

            for (Int32 level = 1; level <= this.PointerDepth; level++)
            {
                ConcurrentDictionary<UInt64, UInt64> currentLevelPointers = new ConcurrentDictionary<UInt64, UInt64>();
                IDictionary<UInt64, UInt64> currentPointers = level == this.PointerDepth ? this.ModulePointers : this.HeapPointers;

                Parallel.ForEach(
                    currentPointers,
                    SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                    (pointer) =>
                {
                    // Accept this pointer if it is points to the previous level snapshot
                    if (previousLevelSnapshot.ContainsAddress(pointer.Value))
                    {
                        currentLevelPointers[pointer.Key] = pointer.Value;
                    }

                    lock (this.ProgressLock)
                    {
                        processedPointers++;

                        // Limit how often we update the progress
                        if (processedPointers % 10000 == 0)
                        {
                            this.UpdateProgress((processedPointers / this.PointerDepth).ToInt32(), this.HeapPointers.Count, canFinalize: false);
                        }
                    }
                });

                previousLevelSnapshot = new Snapshot();

                IList<SnapshotRegion> levelRegions = new List<SnapshotRegion>();

                foreach (KeyValuePair<UInt64, UInt64> pointer in currentLevelPointers)
                {
                    SnapshotRegion levelRegion = new SnapshotRegion(pointer.Key.ToIntPtr(), 1);
                    levelRegion.Expand(this.PointerRadius);
                    levelRegions.Add(levelRegion);
                }

                previousLevelSnapshot.AddSnapshotRegions(levelRegions);

                // Add the pointers for this level to the global accepted list
                this.LevelPointers.Push(currentLevelPointers);
            }
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.LevelPointersCallback?.Invoke(this.LevelPointers);

            this.HeapPointers = null;
            this.LevelPointers = null;

            this.UpdateProgress(ScheduledTask.MaximumProgress);
        }

        private void SetModulePointers(IDictionary<UInt64, UInt64> modulePointers)
        {
            this.ModulePointers = modulePointers;
        }

        private void SetHeapPointers(IDictionary<UInt64, UInt64> heapPointers)
        {
            this.HeapPointers = heapPointers;
        }
    }
    //// End class
}
//// End namespace