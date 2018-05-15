namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Constrols pagination over pointer scan results.
    /// </summary>
    public static class PaginationBuilder
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Pagination Builder";

        /// <summary>
        /// Filters the given snapshot to find all values that are valid pointers.
        /// </summary>
        /// <param name="snapshotFrom">The snapshot from which the algorithm begins. Generally a snapshot of static memory.</param>
        /// <param name="snapshotThrough">The snapshot through which to search. Generally a snapshot of heap memory.</param>
        /// <param name="snapshotTo">The destination snapshot. Generally contains one address.</param>
        /// <param name="dataType">The data type of the pointers.</param>
        /// <param name="depth">The search depth.</param>
        /// <returns></returns>
        public static TrackableTask<PageData> Build(IList<Snapshot> levels, UInt32 radius, DataType dataType)
        {
            TrackableTask<PageData> trackedPointerTreeTask = new TrackableTask<PageData>(PaginationBuilder.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<PageData> builderTask = Task.Factory.StartNew(() =>
            {
                PageData pointerCollection = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    PaginationBuilder.BuildPaginationTable(levels, dataType, radius);

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Pagination complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Pagination canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error paginating", ex);
                    return null;
                }

                return pointerCollection;
            }, cancellationTokenSource.Token);

            trackedPointerTreeTask.SetTrackedTask(builderTask, cancellationTokenSource);

            return trackedPointerTreeTask;
        }

        private static void BuildPaginationTable(IList<Snapshot> levels, DataType dataType, UInt32 radius, Int32 depth = 0, Int32 pageSize = 100, UInt64 pointer = 0, UInt64 universalIndex = 0)
        {
            Snapshot currentSnapshot = levels[depth];
            Snapshot nextSnapshot = depth + 1 >= levels.Count ? null : levels[depth + 1];

            Action recurse = () =>
            {
                UInt64 lowerBoundAddress = unchecked((UInt32)pointer.Subtract(radius, wrapAround: false));
                UInt64 upperBoundAddress = unchecked((UInt32)pointer.Add(radius, wrapAround: false));

                // Enumerate next level to find offsets within range
                foreach (SnapshotRegion region in nextSnapshot.SnapshotRegions)
                {
                    IList<SnapshotRegion> childRegions = PaginationBuilder.FindChildRegions(pointer, lowerBoundAddress, upperBoundAddress, region);

                    if (childRegions.IsNullOrEmpty())
                    {
                        // This should not be possible?
                        continue;
                    }

                    // Enumerate discovered offsets
                    foreach (SnapshotRegion childRegion in childRegions)
                    {
                        IEnumerator<SnapshotElementIndexer> indexer = childRegion.IterateElements();

                        while (indexer.MoveNext())
                        {
                            // Get new address
                            //// Int32 offset = address > indexer.Current.BaseAddress ? unchecked((Int32)(address - indexer.Current.BaseAddress)) : unchecked((Int32)(indexer.Current.BaseAddress - address));
                            UInt64 branchAddress = dataType == DataType.UInt32 ? (UInt32)indexer.Current.LoadCurrentValue() : (UInt64)indexer.Current.LoadCurrentValue();

                            if (depth + 1 < levels.Count)
                            {
                                // Recurse
                                PaginationBuilder.BuildPaginationTable(levels, dataType, radius, depth + 1, pageSize, branchAddress, 0);
                            }
                            else
                            {
                                // Case where 1 child -- add this leaf
                            }
                        }
                    }
                }
            };

            if (depth == 0)
            {
                foreach (SnapshotRegion region in currentSnapshot.SnapshotRegions)
                {
                    IEnumerator<SnapshotElementIndexer> indexer = region.IterateElements();

                    while (indexer.MoveNext())
                    {
                        pointer = dataType == DataType.UInt32 ? (UInt32)indexer.Current.LoadCurrentValue() : (UInt64)indexer.Current.LoadCurrentValue();
                        recurse();
                    }
                }
            }
            else
            {
                if (nextSnapshot != null)
                {
                    recurse();
                }
                else
                {
                    // Case where no children -- just add this leaf
                }
            }
        }

        private static IList<SnapshotRegion> FindChildRegions(UInt64 address, UInt64 lowerBoundAddress, UInt64 upperBoundAddress, SnapshotRegion region)
        {
            SnapshotElementVectorComparer vectorComparer = new SnapshotElementVectorComparer(region: region);

            Vector<UInt32> lowerBound = new Vector<UInt32>(unchecked((UInt32)lowerBoundAddress));
            Vector<UInt32> upperBound = new Vector<UInt32>(unchecked((UInt32)upperBoundAddress));

            // Determines if the addess of an element falls within pointer range
            vectorComparer.SetCustomCompareAction(new Func<Vector<Byte>>(() =>
            {
                Vector<UInt32> currentAddress = new Vector<UInt32>(unchecked((UInt32)vectorComparer.CurrentAddress));

                return Vector.AsVectorByte(Vector.BitwiseAnd(
                        Vector.GreaterThanOrEqual(currentAddress, lowerBound),
                        Vector.LessThanOrEqual(currentAddress, upperBound)));
            }));

            return vectorComparer.Compare();
        }
    }
    //// End class
}
//// End namespace