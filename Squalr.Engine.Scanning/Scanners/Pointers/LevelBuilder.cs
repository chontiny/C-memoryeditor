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
        /// <param name="snapshotTo">The destination snapshot. Generally contains one address.</param>
        /// <param name="depth">The search depth.</param>
        /// <param name="dataType">The data type of the pointers.</param>
        /// <returns></returns>
        public static TrackableTask<IList<Snapshot>> Build(Snapshot snapshotFrom, Snapshot snapshotTo, Int32 depth, UInt32 radius, DataType dataType)
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

                    // Collect all values in the target process
                    Snapshot snapshotAll = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromUserModeMemory, dataType);
                    TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(snapshotAll);
                    snapshotAll = valueCollector.Result;

                    // Counter-intuitively, we begin at the destination and work our way backwards, as it is significantly faster and results in less dead-end pointers
                    IList<Snapshot> backTraceLevels = new List<Snapshot>();
                    Snapshot currentTargetSnapshot = snapshotTo;
                    Snapshot currentSourceSnapshot = snapshotAll;

                    // Step 1) Back trace (we do not care about static/heap at this point)
                    for (Int32 currentDepth = 0; currentDepth < depth; currentDepth++)
                    {
                        TrackableTask<Snapshot> filterTask = PointerFilter.Filter(currentSourceSnapshot, currentTargetSnapshot, radius, dataType);
                        Snapshot pointers = filterTask.Result;

                        if (pointers.ByteCount <= 0)
                        {
                            break;
                        }

                        backTraceLevels.Add(pointers);
                        currentTargetSnapshot = pointers;
                    }

                    currentSourceSnapshot = snapshotFrom;

                    // Step 2) Front trace, starting from static
                    foreach (Snapshot nextSnapshot in backTraceLevels.Reverse())
                    {
                        TrackableTask<Snapshot> heapFilterTask = PointerFilter.Filter(currentSourceSnapshot, nextSnapshot, radius, dataType);
                        Snapshot pointers = heapFilterTask.Result;

                        if (pointers.ByteCount <= 0)
                        {
                            break;
                        }

                        levels.Add(pointers);
                    }

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Level builder complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Level builder canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error building levels", ex);
                    return null;
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