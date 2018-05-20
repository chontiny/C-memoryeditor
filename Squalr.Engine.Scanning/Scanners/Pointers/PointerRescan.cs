namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels;
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using static Squalr.Engine.TrackableTask;

    /// <summary>
    /// Scans for pointers in the target process.
    /// </summary>
    public static class PointerRescan
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Pointer Rescan";

        /// <summary>
        /// Performs a pointer scan for a given address.
        /// </summary>
        /// <param name="address">The address for which to perform a pointer scan.</param>
        /// <param name="maxOffset">The maximum pointer offset.</param>
        /// <param name="depth">The maximum pointer search depth.</param>
        /// <param name="alignment">The pointer scan alignment.</param>
        /// <returns>Atrackable task that returns the scan results.</returns>
        public static TrackableTask<PointerBag> Scan(UInt64 address, PointerBag oldPointerBag)
        {
            TrackableTask<PointerBag> pointerScanTask = TrackableTask<PointerBag>.Create(PointerRescan.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken);

            return pointerScanTask.With(Task.Factory.StartNew<PointerBag>(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        IList<Level> levels = new List<Level>();

                        // Collect all new values
                        foreach (Level level in oldPointerBag.Levels)
                        {
                            // Step 1) Re-read values of all pointers
                            TrackableTask<Snapshot> staticValueCollector = ValueCollector.CollectValues(level.StaticPointers);
                            TrackableTask<Snapshot> heapValueCollector = ValueCollector.CollectValues(level.HeapPointers);

                            Snapshot updatedStaticPointers = staticValueCollector.Result;
                            Snapshot updatedHeapPointers = staticValueCollector.Result;

                            // Step 2) Filter static pointers that still point into the updated heap
                            IVectorSearchKernel staticSearchKernel = SearchKernelFactory.GetSearchKernel(level.HeapPointers, oldPointerBag.MaxOffset, oldPointerBag.PointerSize);
                            TrackableTask<Snapshot> staticFilterTask = PointerFilter.Filter(pointerScanTask, updatedStaticPointers, staticSearchKernel, updatedHeapPointers, oldPointerBag.MaxOffset);

                            Snapshot newStaticPointers = staticFilterTask.Result;

                            levels.Add(new Level(updatedHeapPointers, newStaticPointers));
                        }

                        // Exit if canceled
                        cancellationToken.ThrowIfCancellationRequested();

                        PointerBag pointerBag = new PointerBag(levels, oldPointerBag.MaxOffset, oldPointerBag.PointerSize);

                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Pointer rescan complete in: " + stopwatch.Elapsed);

                        return pointerBag;
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Pointer rescan canceled", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error performing pointer rescan", ex);
                    }

                    return null;
                }, cancellationToken));
        }
    }
    //// End class
}
//// End namespace