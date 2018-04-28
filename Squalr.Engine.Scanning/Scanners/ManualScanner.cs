namespace Squalr.Engine.Scanning.Scanners
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Constraints;
    using Squalr.Engine.Snapshots;
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
        /// Begins the manual scan based on the provided snapshot and parameters.
        /// </summary>
        /// <param name="snapshot">The snapshot on which to perfrom the scan.</param>
        /// <param name="dataType">The data type for which to scan.</param>
        /// <param name="scanConstraintCollection">The collection of scan constraints to use in the manual scan.</param>
        /// <param name="onProgressUpdate">The progress update callback.</param>
        /// <param name="cancellationTokenSource">A token for canceling the scan.</param>
        /// <returns></returns>
        public static Task<Snapshot> Scan(Snapshot snapshot, DataType dataType, ScanConstraintCollection scanConstraintCollection, OnProgressUpdate onProgressUpdate, out CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            snapshot = ValueCollector.CollectValues(snapshot, dataType, null, out _).Result;

            return Task.Factory.StartNew<Snapshot>(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Int32 processedPages = 0;
                Int32 regionCount = snapshot.RegionCount;
                ConcurrentBag<IList<SnapshotRegion>> regions = new ConcurrentBag<IList<SnapshotRegion>>();
                Parallel.ForEach(

                snapshot.OptimizedSnapshotRegions,
                ParallelSettings.ParallelSettingsFastest,
                (region) =>
                {
                    // Perform comparisons
                    IList<SnapshotRegion> results = region.CompareAll(scanConstraintCollection);

                    if (!results.IsNullOrEmpty())
                    {
                        regions.Add(results);
                    }

                    // Update progress every N regions
                    if (onProgressUpdate != null && Interlocked.Increment(ref processedPages) % 32 == 0)
                    {
                        // Check for canceled scan
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        onProgressUpdate(processedPages / regionCount);
                    }
                });
                //// End foreach Region

                // Exit if canceled
                cancellationToken.ThrowIfCancellationRequested();

                Snapshot result = new Snapshot("Manual Scan", regions.SelectMany(region => region));
                stopwatch.Stop();
                Logger.Log(LogLevel.Info, "Scan complete in: " + stopwatch.Elapsed);

                return result;
            }, cancellationTokenSource.Token);
        }
    }
    //// End class
}
//// End namespace