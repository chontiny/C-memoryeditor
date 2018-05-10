namespace Squalr.Engine.Scanning.Scanners
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Constraints;
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
    /// A memory scanning class for classic manual memory scanning techniques.
    /// </summary>
    public static class ManualScanner
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Manual Scanner";

        /// <summary>
        /// Begins the manual scan based on the provided snapshot and parameters.
        /// </summary>
        /// <param name="snapshot">The snapshot on which to perfrom the scan.</param>
        /// <param name="scanConstraintCollection">The collection of scan constraints to use in the manual scan.</param>
        /// <param name="onProgressUpdate">The progress update callback.</param>
        /// <param name="cancellationTokenSource">A token for canceling the scan.</param>
        /// <returns></returns>
        public static TrackableTask<Snapshot> Scan(Snapshot snapshot, ScanConstraintCollection scanConstraintCollection)
        {
            TrackableTask<Snapshot> trackedScanTask = new TrackableTask<Snapshot>(ManualScanner.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<Snapshot> scanTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                Snapshot result = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Int32 processedPages = 0;
                    Int32 regionCount = snapshot.RegionCount;
                    ConcurrentBag<IList<SnapshotRegion>> regions = new ConcurrentBag<IList<SnapshotRegion>>();

                    ParallelOptions options = ParallelSettings.ParallelSettingsFastest.Clone();
                    options.CancellationToken = cancellationTokenSource.Token;

                    Parallel.ForEach(
                        snapshot.OptimizedSnapshotRegions,
                        options,
                        (region) =>
                        {
                            // Check for canceled scan
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            // Perform comparisons
                            IList<SnapshotRegion> results = region.CompareAll(scanConstraintCollection);

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
                    //// End foreach Region

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    result = new Snapshot(ManualScanner.Name, regions.SelectMany(region => region));
                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Scan complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Scan canceled", ex);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error performing scan", ex);
                }

                return result;
            }, cancellationTokenSource.Token);

            trackedScanTask.SetTrackedTask(scanTask, cancellationTokenSource);

            return trackedScanTask;
        }
    }
    //// End class
}
//// End namespace