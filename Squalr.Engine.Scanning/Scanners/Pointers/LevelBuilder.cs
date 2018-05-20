namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.DataTypes;
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
    /// Validates a snapshot of pointers.
    /// </summary>
    internal static class LevelBuilder
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Level Builder";

        /// <summary>
        /// Filters the given snapshot to find all values that are valid pointers.
        /// </summary>
        /// <param name="snapshotStatic">The snapshot from which the algorithm begins. Generally a snapshot of static memory.</param>
        /// <param name="snapshotDestination">The destination snapshot. Generally contains one address.</param>
        /// <param name="depth">The search depth.</param>
        /// <param name="dataType">The data type of the pointers.</param>
        /// <returns></returns>
        public static TrackableTask<IList<Level>> Build(TrackableTask parentTask, Snapshot snapshotStatic, Snapshot snapshotHeaps, Snapshot snapshotDestination, Int32 depth, UInt32 radius, DataType dataType)
        {
            return TrackableTask<IList<Level>>
                .Create(LevelBuilder.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken)
                .With(Task.Factory.StartNew<IList<Level>>(() =>
                {
                    try
                    {
                        IList<Level> levels = new List<Level>();

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        levels.Add(new Level(snapshotDestination));

                        // Counter-intuitively, we begin at the destination and work our way backwards, as it results in less dead-end pointers
                        for (Int32 currentDepth = 0; currentDepth < depth; currentDepth++)
                        {
                            // Exit if canceled
                            parentTask.CancellationToken.ThrowIfCancellationRequested();

                            Stopwatch levelStopwatch = new Stopwatch();
                            levelStopwatch.Start();

                            Level previousLevel = levels.Last();

                            // Build static pointers
                            IVectorSearchKernel staticSearchKernel = SearchKernelFactory.GetSearchKernel(previousLevel.HeapPointers, radius);
                            TrackableTask<Snapshot> staticFilterTask = PointerFilter.Filter(parentTask, snapshotStatic, staticSearchKernel, previousLevel.HeapPointers, radius);

                            // Build the heap pointers for the next level for all but the last level
                            if (currentDepth < depth - 1)
                            {
                                IVectorSearchKernel heapSearchKernel = SearchKernelFactory.GetSearchKernel(levels.Last().HeapPointers, radius);
                                TrackableTask<Snapshot> heapFilterTask = PointerFilter.Filter(parentTask, snapshotHeaps, heapSearchKernel, levels.Last().HeapPointers, radius);

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
                            parentTask.Progress = ((float)(currentDepth + 1)) / (float)depth * 100.0f;
                        }

                        // Exit if canceled
                        parentTask.CancellationToken.ThrowIfCancellationRequested();

                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Level builder complete in: " + stopwatch.Elapsed);

                        return levels;
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Level builder canceled", ex);
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error building levels", ex);
                    }

                    return null;
                }, parentTask.CancellationToken));
        }
    }
    //// End class
}
//// End namespace