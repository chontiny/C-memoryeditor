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
    using System.Linq;
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
        public static TrackableTask<PointerCollection> Build(IList<Snapshot> levels, UInt32 radius, DataType dataType)
        {
            TrackableTask<PointerCollection> trackedPointerTreeTask = new TrackableTask<PointerCollection>(TreeBuilder.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<PointerCollection> builderTask = Task.Factory.StartNew(() =>
            {
                PointerCollection pointerCollection = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Snapshot staticPointers = levels.FirstOrDefault();
                    IEnumerable<Snapshot> heapLevels = levels.Skip(1);

                    if (staticPointers == null || heapLevels.Count() <= 0)
                    {
                        return null;
                    }

                    pointerCollection = TreeBuilder.BuildPointers(staticPointers, radius, heapLevels.ToArray(), dataType);

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

        private static PointerCollection BuildPointers(Snapshot staticPointers, UInt32 radius, Snapshot[] heapSnapshots, DataType dataType, Int32 depth = 0)
        {
            PointerCollection pointerCollection = new PointerCollection();

            Parallel.ForEach(
            staticPointers.OptimizedSnapshotRegions,
            ParallelSettings.ParallelSettingsFastest,
            (staticRegion) =>
            {
                IEnumerator<SnapshotElementIndexer> indexer = staticRegion.IterateElements();

                while (indexer.MoveNext())
                {
                    PointerRoot pointerRoot = new PointerRoot(indexer.Current.BaseAddress, unchecked((UInt32)(staticRegion.BaseAddress) + unchecked((UInt32)(indexer.Current.ElementIndex))));

                    pointerRoot.Branches = BuildBranches(pointerRoot.BaseAddress, radius, heapSnapshots, dataType);

                    pointerCollection.PointerRoots.Add(pointerRoot);
                }
            });

            return pointerCollection;
        }

        private static IEnumerable<PointerBranch> BuildBranches(UInt64 address, UInt32 radius, Snapshot[] snapshots, DataType dataType, Int32 depth = 0)
        {
            if (snapshots.Length < depth)
            {
                return null;
            }

            ConcurrentBag<PointerBranch> branches = new ConcurrentBag<PointerBranch>();
            Snapshot snapshot = snapshots[depth];

            Parallel.ForEach(
            snapshot.OptimizedSnapshotRegions,
            ParallelSettings.ParallelSettingsFastest,
            (region) =>
            {
                if (!region.ReadGroup.CanCompare(hasRelativeConstraint: false))
                {
                    return;
                }

                SnapshotElementVectorComparer vectorComparer = new SnapshotElementVectorComparer(region: region);

                // Determines if the addess of an element falls within pointer range
                vectorComparer.SetCustomCompareAction(new Func<Vector<Byte>>(() =>
                {
                    Vector<UInt32> lowerBound = new Vector<UInt32>(unchecked((UInt32)address) - radius);
                    Vector<UInt32> upperBound = new Vector<UInt32>(unchecked((UInt32)address) + radius);
                    Vector<UInt32> currentAddress = new Vector<UInt32>(unchecked((UInt32)vectorComparer.CurrentAddress));

                    return Vector.AsVectorByte(Vector.BitwiseAnd(
                            Vector.GreaterThanOrEqual(currentAddress, lowerBound),
                            Vector.LessThanOrEqual(currentAddress, upperBound)));
                }));

                IList<SnapshotRegion> results = vectorComparer.Compare();

                if (results.IsNullOrEmpty())
                {
                    return;
                }

                // Iterate results to build branches
                foreach (SnapshotRegion resultRegion in results)
                {
                    IEnumerator<SnapshotElementIndexer> indexer = resultRegion.IterateElements();

                    while (indexer.MoveNext())
                    {
                        Int32 offset = address > indexer.Current.BaseAddress ? unchecked((Int32)(address - indexer.Current.BaseAddress)) : unchecked((Int32)(indexer.Current.BaseAddress - address));
                        UInt64 branchAddress = dataType == DataType.UInt32 ? (UInt32)indexer.Current.LoadCurrentValue() : (UInt64)indexer.Current.LoadCurrentValue();
                        PointerBranch branch = new PointerBranch(offset, branchAddress);

                        // Recurse
                        if (depth + 1 < snapshots.Length)
                        {
                            branch.Branches = TreeBuilder.BuildBranches(branchAddress, radius, snapshots, dataType, depth + 1);
                        }

                        branches.Add(branch);
                    }
                }
            });

            return branches;
        }
    }
    //// End class
}
//// End namespace