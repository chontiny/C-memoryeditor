namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Engine;
    using Squalr.Engine.Memory;
    using Squalr.Engine.Output;
    using Squalr.Engine.Processes;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Snapshots;
    using Squalr.Source.Utils.Extensions;
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
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Boolean isProcess32Bit = ProcessAdapterFactory.GetProcessAdapter().IsOpenedProcess32Bit();
            Int32 vectorSize = Eng.GetInstance().Architecture.GetVectorSize();
            UInt64 minValue = UInt16.MaxValue;
            UInt64 maxValue = Eng.GetInstance().VirtualMemory.GetMaxUsermodeAddress();
            Vector<UInt32> minValueVector = new Vector<UInt32>(minValue.ToUInt32());
            Vector<UInt32> maxValueVector = new Vector<UInt32>(maxValue.ToUInt32());
            Vector<UInt64> minValueVector64 = new Vector<UInt64>(minValue);
            Vector<UInt64> maxValueVector64 = new Vector<UInt64>(maxValue);

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

                foreach (SnapshotRegion region in snapshot.SnapshotRegions)
                {
                    if (region.ReadGroup.CurrentValues.IsNullOrEmpty())
                    {
                        continue;
                    }

                    if (isProcess32Bit)
                    {
                        Parallel.For(0, region.RegionSize / vectorSize, (elementIndex) =>
                        {
                            Int32 vectorReadIndex = elementIndex * vectorSize;

                            Vector<UInt32> nextElements = Vector.AsVectorUInt32(new Vector<Byte>(region.ReadGroup.CurrentValues, unchecked((Int32)vectorReadIndex)));

                            // Check if all elements contain common invalid values (~70-80% of values will simply be 0)
                            if (Vector.LessThanAll(nextElements, minValueVector) || Vector.GreaterThanAll(nextElements, minValueVector))
                            {
                                return;
                            }
                            else
                            {
                                // Vector contains valid elements. These must be processed normally.
                                for (Int32 index = 0; index < vectorSize / sizeof(UInt32); index++)
                                {
                                    UInt32 pointerDestination = nextElements[index];

                                    // Check for common invalid values again, but for this specific element
                                    if (pointerDestination < minValue || pointerDestination > maxValue)
                                    {
                                        continue;
                                    }

                                    // Check if it is possible that this pointer is valid -- if so keep it
                                    if (heapSnapshot.ContainsAddress(pointerDestination))
                                    {
                                        UInt32 pointerAddress = region.BaseAddress.ToUInt32() + unchecked((UInt32)(vectorReadIndex + index * sizeof(UInt32)));

                                        if (isModule)
                                        {
                                            this.ModulePointers[pointerAddress] = pointerDestination;
                                        }
                                        else
                                        {
                                            this.HeapPointers[pointerAddress] = pointerDestination;
                                        }
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        Parallel.For(0, region.RegionSize / vectorSize, (elementIndex) =>
                        {
                            Int32 vectorReadIndex = elementIndex * vectorSize;

                            Vector<UInt64> nextElements = Vector.AsVectorUInt64(new Vector<Byte>(region.ReadGroup.CurrentValues, unchecked((Int32)vectorReadIndex)));

                            // Check if all elements contain common invalid values (~70-80% of values will simply be 0)
                            if (Vector.LessThanAll(nextElements, minValueVector64) || Vector.GreaterThanAll(nextElements, maxValueVector64))
                            {
                                return;
                            }
                            else
                            {
                                // Vector contains valid elements. These must be processed normally.
                                for (Int32 index = 0; index < vectorSize / sizeof(UInt64); index++)
                                {
                                    UInt64 pointerDestination = nextElements[index];

                                    // Check for common invalid values again, but for this specific element
                                    if (pointerDestination < minValue || pointerDestination > maxValue)
                                    {
                                        continue;
                                    }

                                    // Check if it is possible that this pointer is valid -- if so keep it
                                    if (heapSnapshot.ContainsAddress(pointerDestination))
                                    {
                                        UInt64 pointerAddress = region.BaseAddress.ToUInt64() + (vectorReadIndex + index * sizeof(UInt64)).ToUInt64();

                                        if (isModule)
                                        {
                                            this.ModulePointers[pointerAddress] = pointerDestination;
                                        }
                                        else
                                        {
                                            this.HeapPointers[pointerAddress] = pointerDestination;
                                        }
                                    }
                                }
                            }
                        });
                    }

                    // Clear the saved values, we do not need them now
                    region.ReadGroup.SetCurrentValues(null);

                    // Update scan progress
                    if (Interlocked.Increment(ref processedRegions) % 32 == 0)
                    {
                        this.UpdateProgress(processedRegions, regionCount, canFinalize: false);
                    }
                }
            }

            stopwatch.Stop();
            Output.Log(LogLevel.Info, "Pointers collected in: " + stopwatch.Elapsed);
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