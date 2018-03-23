namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Properties;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Utils.Extensions;
    using System;
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
        public PointerBackTracer(Action<LevelPointers> levelPointersCallback) : base(
            taskName: "Pointer Back Trace",
            isRepeated: false,
            trackProgress: true)
        {
            this.LevelPointersCallback = levelPointersCallback;

            this.Dependencies.Enqueue(new PointerCollector(this.SetCollectedPointers));
        }

        /// <summary>
        /// Gets or sets the pointers residing in a module.
        /// </summary>
        private PointerPool ModulePointers { get; set; }

        /// <summary>
        /// Gets or sets the pointers residing in the heap.
        /// </summary>
        private PointerPool HeapPointers { get; set; }

        /// <summary>
        /// Gets or sets the pointer depth of the scan.
        /// </summary>
        public Int32 PointerDepth { get; set; }

        /// <summary>
        /// Gets or sets the pointer radius of the scan.
        /// </summary>
        public Int32 PointerRadius { get; set; }

        /// <summary>
        /// Gets or sets the target address of the pointer scan.
        /// </summary>
        public UInt64 TargetAddress { get; set; }

        private LevelPointers LevelPointers { get; set; }

        private Action<LevelPointers> LevelPointersCallback { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
            this.UpdateInterval = PointerBackTracer.RescanTime;

            this.LevelPointers = new LevelPointers();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Int64 processedPointers = 0;

            // Create a snapshot only containing the destination (as this is a back-tracing algorithm, we work backwards from the destination)
            Snapshot previousLevelSnapshot = new Snapshot(null, new ReadGroup[] { new ReadGroup(this.TargetAddress.ToIntPtr().Subtract(this.PointerRadius, wrapAround: false), this.PointerRadius) });

            // Find all pointers that point to the previous level
            for (Int32 level = 0; level < this.PointerDepth; level++)
            {
                Boolean isModuleLevel = level == this.PointerDepth - 1;
                PointerPool currentLevelPointers = new PointerPool();

                // Check if any pointers point to the previous level
                Parallel.ForEach(
                    isModuleLevel ? this.ModulePointers : this.HeapPointers,
                    SettingsViewModel.GetInstance().ParallelSettingsFastest,
                    (pointer) =>
                    {
                        // Accept this pointer if it is points to the previous level snapshot
                        if (previousLevelSnapshot.ContainsAddress(pointer.Value))
                        {
                            currentLevelPointers[pointer.Key] = pointer.Value;
                        }

                        // Update scan progress
                        if (Interlocked.Increment(ref processedPointers) % 1024 == 0)
                        {
                            this.UpdateProgress((processedPointers / unchecked((UInt32)this.PointerDepth)).ToInt32(), this.HeapPointers.Count + this.ModulePointers.Count, canFinalize: false);
                        }
                    });

                // Create a snapshot from this level of pointers
                previousLevelSnapshot = currentLevelPointers.ToSnapshot(this.PointerRadius);

                // Add the pointer pool to the level structure
                if (isModuleLevel)
                {
                    this.LevelPointers.ModulePointerPool = currentLevelPointers;
                }
                else
                {
                    this.LevelPointers.AddHeapPointerPool(currentLevelPointers);
                }
            }

            // Add the destination pointer pool
            this.LevelPointers.DestinationPointerPool = new PointerPool(new KeyValuePair<UInt64, UInt64>(this.TargetAddress, 0));
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

        /// <summary>
        /// Callback function from the pointer collector to retrieve the collected pointers.
        /// </summary>
        /// <param name="modulePointers">The collected pointers residing in modules.</param>
        /// <param name="heapPointers">The collected pointers residing in the heap.</param>
        private void SetCollectedPointers(PointerPool modulePointers, PointerPool heapPointers)
        {
            this.ModulePointers = modulePointers;
            this.HeapPointers = heapPointers;
        }
    }
    //// End class
}
//// End namespace