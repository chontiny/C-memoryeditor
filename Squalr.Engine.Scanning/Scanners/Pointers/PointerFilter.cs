using Squalr.Engine.DataTypes;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.OS;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Validates a snapshot of pointers.
    /// </summary>
    public static class PointerFilter
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
        public static TrackableTask<Snapshot> Filter(Snapshot snapshot, Snapshot boundsSnapshot, UInt32 radius, DataType dataType)
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

                    IEnumerable<UInt32> tempLowerBounds = boundsSnapshot.SnapshotRegions.Select(region => unchecked((UInt32)region.BaseAddress.Subtract(radius, wrapAround: false)));
                    IEnumerable<UInt32> tempUpperBounds = boundsSnapshot.SnapshotRegions.Select(region => unchecked((UInt32)region.EndAddress.Add(radius, wrapAround: false)));

                    while (tempLowerBounds.Count() % vectorSize != 0)
                    {
                        tempLowerBounds = tempLowerBounds.Append<UInt32>(UInt32.MaxValue);
                        tempUpperBounds = tempUpperBounds.Append<UInt32>(UInt32.MinValue);
                    }

                    UInt32[] lowerBounds = tempLowerBounds.ToArray();
                    UInt32[] upperBounds = tempUpperBounds.ToArray();

                    Int32 processedPages = 0;
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

                            if (!region.ReadGroup.CanCompare(hasRelativeConstraint: false))
                            {
                                return;
                            }

                            SnapshotElementVectorComparer vectorComparer = new SnapshotElementVectorComparer(region: region);

                            vectorComparer.SetCustomCompareAction(new Func<Vector<Byte>>(() =>
                            {
                                Vector<UInt32> result = Vector<UInt32>.Zero;

                                // Perform vectorized linear search. This is why you should not trust big O notation -- this severely beats scalar binary search
                                for (Int32 boundsIndex = 0; boundsIndex < lowerBounds.Length; boundsIndex += vectorSize)
                                {
                                    Vector<UInt32> nextLowerBounds = new Vector<UInt32>(lowerBounds, boundsIndex);
                                    Vector<UInt32> nextUpperBounds = new Vector<UInt32>(upperBounds, boundsIndex);

                                    result = Vector.BitwiseOr(result,
                                        Vector.BitwiseAnd(
                                            Vector.GreaterThanOrEqual(Vector.AsVectorUInt32(vectorComparer.CurrentValues), nextLowerBounds),
                                            Vector.LessThanOrEqual(Vector.AsVectorUInt32(vectorComparer.CurrentValues), nextUpperBounds)));
                                }

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