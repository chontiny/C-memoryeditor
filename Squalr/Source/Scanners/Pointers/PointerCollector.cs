namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Properties;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Snapshots;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collects all module and heap pointers in the running process.
    /// </summary>
    internal class PointerCollector : ScheduledTask
    {
        /// <summary>
        /// The rounding size for pointer destinations.
        /// </summary>
        private const Int32 PointerRoundingSize = 1024;

        /// <summary>
        /// Creates an instance of the <see cref="PointerCollector" /> class.
        /// </summary>
        public PointerCollector(Action<PointerPool, PointerPool> collectedPointersCallback) : base(
            taskName: "Pointer Collector",
            isRepeated: false,
            trackProgress: true)
        {
            this.CollectedPointersCallback = collectedPointersCallback;
        }

        /// <summary>
        /// Gets or sets the collected module pointers.
        /// </summary>
        private PointerPool ModulePointers { get; set; }

        /// <summary>
        /// Gets or sets the collected heap pointers.
        /// </summary>
        private PointerPool HeapPointers { get; set; }

        /// <summary>
        /// Gets or sets the callback function to which we pass the collected pointers.
        /// </summary>
        private Action<PointerPool, PointerPool> CollectedPointersCallback;

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
            this.ModulePointers = new PointerPool();
            this.HeapPointers = new PointerPool();

            Boolean isProcess32Bit = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit();
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Boolean isProcess32Bit = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit();
            Int32 vectorSize = EngineCore.GetInstance().Architecture.GetVectorSize();
            Vector<UInt64> minValue = new Vector<UInt64>(UInt16.MaxValue);
            Vector<UInt64> maxValue = new Vector<UInt64>(EngineCore.GetInstance().VirtualMemory.GetMaxUsermodeAddress());

            Snapshot moduleSnapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromModules);
            Snapshot heapSnapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromHeap);
            List<Snapshot> snapshots = new List<Snapshot>(new Snapshot[] { moduleSnapshot, heapSnapshot });

            moduleSnapshot.ReadAllMemory();
            heapSnapshot.ReadAllMemory();

            Int32 processedRegions = 0;
            Int32 regionCount = moduleSnapshot.RegionCount + heapSnapshot.RegionCount;

            // Process the allowed amount of chunks from the priority queue
            foreach (Snapshot snapshot in snapshots)
            {
                Boolean isModule = snapshot == moduleSnapshot;

                Parallel.ForEach(
                snapshot.SnapshotRegions,
                SettingsViewModel.GetInstance().ParallelSettingsFastest,
                (region) =>
                {
                    if (region.ReadGroup.CurrentValues.IsNullOrEmpty())
                    {
                        return;
                    }

                    for (Int32 vectorReadIndex = 0; vectorReadIndex < region.RegionSize; vectorReadIndex += vectorSize)
                    {
                        Vector<UInt64> nextElements = Vector.AsVectorUInt64(new Vector<Byte>(region.ReadGroup.CurrentValues, unchecked((Int32)vectorReadIndex)));

                        // Check for common invalid values (~70-80% of values will simply be 0)
                        if (Vector.LessThanAll(nextElements, minValue) || Vector.GreaterThanAll(nextElements, maxValue))
                        {
                            continue;
                        }
                        else
                        {
                            // Vector contains valid elements. These must be processed normally.
                            for (Int32 index = 0; index < vectorSize / sizeof(UInt64); index++)
                            {
                                UInt64 candidatePointer = nextElements[index];

                                // Check if it is possible that this pointer is valid, if so keep it
                                if (heapSnapshot.ContainsAddress(candidatePointer))
                                {
                                    if (isModule)
                                    {
                                        this.ModulePointers[region.BaseAddress.ToUInt64() + (vectorReadIndex + index).ToUInt64()] = candidatePointer;
                                    }
                                    else
                                    {
                                        this.HeapPointers[region.BaseAddress.ToUInt64() + (vectorReadIndex + index).ToUInt64()] = candidatePointer;
                                    }
                                }
                            }
                        }
                    }

                    // Clear the saved values, we do not need them now
                    region.ReadGroup.SetCurrentValues(null);

                    // Update scan progress
                    if (Interlocked.Increment(ref processedRegions) % 32 == 0)
                    {
                        this.UpdateProgress(processedRegions, regionCount, canFinalize: false);
                    }
                });
            }

            stopwatch.Stop();
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Pointers collected in: " + stopwatch.Elapsed);
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.CollectedPointersCallback?.Invoke(this.ModulePointers, this.HeapPointers);

            this.UpdateProgress(ScheduledTask.MaximumProgress);

            this.ModulePointers = null;
            this.HeapPointers = null;
        }
    }
    //// End class
}
//// End namespace