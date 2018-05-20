using Squalr.Engine.OS;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels;
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static Squalr.Engine.TrackableTask;

    /// <summary>
    /// Scans for pointers in the target process.
    /// </summary>
    public static class PointerScan
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Pointer Scan";

        /// <summary>
        /// Performs a pointer scan for a given address.
        /// </summary>
        /// <param name="address">The address for which to perform a pointer scan.</param>
        /// <param name="maxOffset">The maximum pointer offset.</param>
        /// <param name="depth">The maximum pointer search depth.</param>
        /// <param name="alignment">The pointer scan alignment.</param>
        /// <returns>Atrackable task that returns the scan results.</returns>
        public static TrackableTask<PointerBag> Scan(UInt64 address, UInt32 maxOffset, Int32 depth, Int32 alignment)
        {
            TrackableTask<PointerBag> pointerScanTask = TrackableTask<PointerBag>.Create(PointerScan.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken);

            return pointerScanTask.With(Task.Factory.StartNew<PointerBag>(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        PointerSize pointerSize = Processes.Default.IsOpenedProcess32Bit() ? PointerSize.Byte4 : PointerSize.Byte8;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        // Step 1) Create a snapshot of the target address
                        Snapshot targetAddress = new Snapshot(new SnapshotRegion[] { new SnapshotRegion(new ReadGroup(address, pointerSize.ToSize(), pointerSize.ToDataType(), alignment), 0, pointerSize.ToSize()) });

                        // Step 2) Collect static pointers
                        Snapshot snapshotStatic = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromModules, pointerSize.ToDataType());
                        TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(snapshotStatic);
                        snapshotStatic = valueCollector.Result;

                        // Step 3) Collect heap pointers
                        Snapshot snapshotHeaps = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromHeaps, pointerSize.ToDataType());
                        TrackableTask<Snapshot> heapValueCollector = ValueCollector.CollectValues(snapshotHeaps);
                        snapshotHeaps = heapValueCollector.Result;

                        // Step 4) Build levels
                        IList<Level> levels = new List<Level>
                        {
                            new Level(targetAddress)
                        };

                        // Counter-intuitively, we begin at the destination and work our way backwards, as it results in less dead-end pointers
                        for (Int32 currentDepth = 0; currentDepth < depth; currentDepth++)
                        {
                            // Exit if canceled
                            cancellationToken.ThrowIfCancellationRequested();

                            Stopwatch levelStopwatch = new Stopwatch();
                            levelStopwatch.Start();

                            Level previousLevel = levels.Last();

                            // Build static pointers
                            IVectorSearchKernel staticSearchKernel = SearchKernelFactory.GetSearchKernel(previousLevel.HeapPointers, maxOffset, pointerSize);
                            TrackableTask<Snapshot> staticFilterTask = PointerFilter.Filter(pointerScanTask, snapshotStatic, staticSearchKernel, previousLevel.HeapPointers, maxOffset);

                            // Build the heap pointers for the next level for all but the last level
                            if (currentDepth < depth - 1)
                            {
                                IVectorSearchKernel heapSearchKernel = SearchKernelFactory.GetSearchKernel(previousLevel.HeapPointers, maxOffset, pointerSize);
                                TrackableTask<Snapshot> heapFilterTask = PointerFilter.Filter(pointerScanTask, snapshotHeaps, heapSearchKernel, previousLevel.HeapPointers, maxOffset);

                                heapFilterTask.OnCompletedEvent += ((snapshot) =>
                                {
                                    levelStopwatch.Stop();
                                    Logger.Log(LogLevel.Info, "Level " + (depth - currentDepth - 1) + " => " + (depth - currentDepth) + " built in: " + levelStopwatch.Elapsed);
                                });

                                levels.Add(new Level(heapFilterTask.Result));
                            }
                            else
                            {
                                staticFilterTask.OnCompletedEvent += ((snapshot) =>
                                {
                                    levelStopwatch.Stop();
                                    Logger.Log(LogLevel.Info, "Final static level built in: " + levelStopwatch.Elapsed);
                                });
                            }

                            previousLevel.StaticPointers = staticFilterTask.Result;

                            // Update progress
                            updateProgress(((float)(currentDepth + 1)) / (float)depth * 100.0f);
                        }

                        // Exit if canceled
                        cancellationToken.ThrowIfCancellationRequested();

                        // Step 5) Store results in a bag from which pointers can be retrieved
                        PointerBag pointerBag = new PointerBag(levels, maxOffset, pointerSize);

                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Pointer scan complete in: " + stopwatch.Elapsed);

                        return pointerBag;
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Pointer scan canceled", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error performing pointer scan", ex);
                    }

                    return null;
                }, cancellationToken));
        }
    }
    //// End class
}
//// End namespace