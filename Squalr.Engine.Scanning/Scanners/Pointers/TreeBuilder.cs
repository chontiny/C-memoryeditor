namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Validates a snapshot of pointers.
    /// </summary>
    public static class TreeBuilder
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Tree Builder";

        /// <summary>
        /// Filters the given snapshot to find all values that are valid pointers.
        /// </summary>
        /// <param name="snapshotFrom">The snapshot from which the algorithm begins. Generally a snapshot of static memory.</param>
        /// <param name="snapshotThrough">The snapshot through which to search. Generally a snapshot of heap memory.</param>
        /// <param name="snapshotTo">The destination snapshot. Generally contains one address.</param>
        /// <param name="dataType">The data type of the pointers.</param>
        /// <param name="depth">The search depth.</param>
        /// <returns></returns>
        public static TrackableTask<PointerCollection> Build(IList<Snapshot> levels, DataType dataType)
        {
            TrackableTask<PointerCollection> trackedPointerTreeTask = new TrackableTask<PointerCollection>(TreeBuilder.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<PointerCollection> builderTask = Task.Factory.StartNew(() =>
            {
                PointerCollection pointerCollection = new PointerCollection();

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Int32 processedPages = 0;
                    ConcurrentBag<IList<SnapshotRegion>> regions = new ConcurrentBag<IList<SnapshotRegion>>();

                    ParallelOptions options = ParallelSettings.ParallelSettingsFastest.Clone();
                    options.CancellationToken = cancellationTokenSource.Token;

                    foreach (Snapshot snapshot in levels)
                    {
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

                                vectorComparer.SetCustomCompareAction(new Func<Vector<Byte>>(() =>
                                {
                                    Vector<UInt32> result = Vector<UInt32>.Zero;

                                    return Vector.AsVectorByte(result);
                                }));

                                IList<SnapshotRegion> results = vectorComparer.Compare();

                                if (!results.IsNullOrEmpty())
                                {
                                    regions.Add(results);
                                }

                                // Update progress every N regions
                                if (Interlocked.Increment(ref processedPages) % 32 == 0)
                                {
                                    trackedPointerTreeTask.Progress = (float)processedPages / (float)snapshot.RegionCount * 100.0f;
                                }
                            });
                    }

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Tree builder complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Tree builder canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error building trees", ex);
                    return null;
                }

                return pointerCollection;
            }, cancellationTokenSource.Token);

            trackedPointerTreeTask.SetTrackedTask(builderTask, cancellationTokenSource);

            return trackedPointerTreeTask;
        }
    }
    //// End class
}
//// End namespace