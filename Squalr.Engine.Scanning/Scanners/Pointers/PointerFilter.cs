namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.OS;
    using Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Validates a snapshot of pointers.
    /// </summary>
    internal static class PointerFilter
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Pointer Validator";

        /// <summary>
        /// Filters the given snapshot to find all values that are valid pointers.
        /// </summary>
        /// <param name="snapshot">The snapshot on which to perfrom the scan.</param>
        /// <returns></returns>
        public static TrackableTask<Snapshot> Filter(Snapshot snapshot, ISearchKernel searchKernel, Snapshot DEBUG, UInt32 RADIUS_DEBUG)
        {
            TrackableTask<Snapshot> trackedScanTask = new TrackableTask<Snapshot>(PointerFilter.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<Snapshot> filterTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Boolean isProcess32Bit = Processes.Default.IsOpenedProcess32Bit();
                    Int32 vectorSize = Vectors.VectorSize;

                    Int32 processedPages = 0;
                    ConcurrentBag<IList<SnapshotRegion>> regions = new ConcurrentBag<IList<SnapshotRegion>>();

                    ParallelOptions options = ParallelSettings.ParallelSettingsFastest.Clone();
                    options.CancellationToken = cancellationTokenSource.Token;

                    // ISearchKernel DEBUG_KERNEL = new SpanSearchKernel(DEBUG, RADIUS_DEBUG);

                    Parallel.ForEach(
                        snapshot.OptimizedSnapshotRegions,
                        options,
                        (region) =>
                        {
                            // Check for canceled scan
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            if (!region.ReadGroup.CanCompare(hasRelativeConstraint: false))
                            {
                                return;
                            }

                            SnapshotElementVectorComparer vectorComparer = new SnapshotElementVectorComparer(region: region);
                            vectorComparer.SetCustomCompareAction(searchKernel.GetSearchKernel(vectorComparer));

                            // SnapshotElementVectorComparer DEBUG_COMPARER = new SnapshotElementVectorComparer(region: region);
                            // DEBUG_COMPARER.SetCustomCompareAction(DEBUG_KERNEL.GetSearchKernel(DEBUG_COMPARER));

                            IList<SnapshotRegion> results = vectorComparer.Compare();
                            // IList<SnapshotRegion> DEBUG_RESULTS = vectorComparer.Compare();

                            if (!results.IsNullOrEmpty())
                            {
                                regions.Add(results);
                            }

                            // Update progress every N regions
                            if (Interlocked.Increment(ref processedPages) % 32 == 0)
                            {
                                trackedScanTask.Progress = (float)processedPages / (float)snapshot.RegionCount * 100.0f;
                            }
                        });

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    snapshot = new Snapshot(PointerFilter.Name, regions.SelectMany(region => region));

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Pointer filtering complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Pointer filtering canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error performing pointer filtering", ex);
                    return null;
                }

                return snapshot;
            }, cancellationTokenSource.Token);

            trackedScanTask.SetTrackedTask(filterTask, cancellationTokenSource);

            return trackedScanTask;
        }
    }
    //// End class
}
//// End namespace