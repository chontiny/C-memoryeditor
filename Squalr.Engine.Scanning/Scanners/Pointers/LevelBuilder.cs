namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Validates a snapshot of pointers.
    /// </summary>
    public static class LevelBuilder
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Level Builder";

        /// <summary>
        /// Filters the given snapshot to find all values that are valid pointers.
        /// </summary>
        /// <param name="snapshotFrom">The snapshot from which the algorithm begins. Generally a snapshot of static memory.</param>
        /// <param name="snapshotThrough">The snapshot through which to search. Generally a snapshot of heap memory.</param>
        /// <param name="snapshotTo">The destination snapshot. Generally contains one address.</param>
        /// <param name="dataType">The data type of the pointers.</param>
        /// <param name="depth">The search depth.</param>
        /// <returns></returns>
        public static TrackableTask<IList<Snapshot>> Build(Snapshot snapshotFrom, Snapshot snapshotThrough, Snapshot snapshotTo, DataType dataType, Int32 depth)
        {
            TrackableTask<IList<Snapshot>> trackedScanTask = new TrackableTask<IList<Snapshot>>(LevelBuilder.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<IList<Snapshot>> builderTask = Task.Factory.StartNew(() =>
            {
                IList<Snapshot> levels = new List<Snapshot>();

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    // Counter-intuitively, we begin at the destination and work our way backwards, as it is significantly faster and results in less dead-end pointers
                    IList<Snapshot> backTraceLevels = new List<Snapshot>();
                    Snapshot targetSnapshot = snapshotTo;
                    Snapshot sourceSnapshot = snapshotThrough;

                    backTraceLevels.Add(targetSnapshot);

                    // Step 1) Back trace
                    for (Int32 currentDepth = 0; currentDepth < depth; currentDepth++)
                    {
                        // For the last level use static snapshot
                        if (currentDepth == depth - 1)
                        {
                            sourceSnapshot = snapshotFrom;
                        }

                        TrackableTask<Snapshot> filterTask = PointerFilter.Filter(sourceSnapshot, targetSnapshot, dataType);
                        Snapshot nextLevel = filterTask.Result;
                        backTraceLevels.Add(nextLevel);

                        targetSnapshot = nextLevel;
                    }

                    targetSnapshot = snapshotFrom;

                    // Step 2) Front trace
                    foreach (Snapshot backTraceLevel in backTraceLevels.Reverse())
                    {
                        TrackableTask<Snapshot> filterTask = PointerFilter.Filter(targetSnapshot, backTraceLevel, dataType);
                        Snapshot nextLevel = filterTask.Result;
                        levels.Add(nextLevel);

                        targetSnapshot = nextLevel;
                    }

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Level builder complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Level builder canceled", ex);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error building levels filtering", ex);
                }

                return levels;
            }, cancellationTokenSource.Token);

            trackedScanTask.SetTrackedTask(builderTask, cancellationTokenSource);

            return trackedScanTask;
        }
    }
    //// End class
}
//// End namespace