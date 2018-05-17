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
        /// <param name="snapshotStatic">The snapshot from which the algorithm begins. Generally a snapshot of static memory.</param>
        /// <param name="snapshotDestination">The destination snapshot. Generally contains one address.</param>
        /// <param name="depth">The search depth.</param>
        /// <param name="dataType">The data type of the pointers.</param>
        /// <returns></returns>
        public static TrackableTask<IList<Snapshot>> Build(Snapshot snapshotStatic, Snapshot snapshotDestination, Int32 depth, UInt32 radius, DataType dataType)
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
                    Snapshot currentTargetSnapshot = snapshotDestination;
                    Snapshot currentSourceSnapshot = snapshotAll;

                    backTraceLevels.Add(snapshotDestination);
                    IEnumerable<Int32> depthRange = Enumerable.Range(0, depth);

                    /*
                    // MANUAL POINTER SCAN AND VALIDATION FOR DEBUGGING:
                    TrackableTask<Snapshot> DEBUGTASK1 = PointerFilter.Filter(snapshotAll, snapshotDestination, radius, dataType);
                    Snapshot L3 = DEBUGTASK1.Result;

                    TrackableTask<Snapshot> DEBUGTASKX = PointerFilter.Filter(snapshotAll, L3, radius, dataType);
                    Snapshot L2 = DEBUGTASKX.Result;

                    TrackableTask<Snapshot> DEBUGTASK2 = PointerFilter.Filter(snapshotStatic, L2, radius, dataType);
                    Snapshot L1 = DEBUGTASK2.Result;

                    TrackableTask<Snapshot> DEBUGTASK3 = PointerFilter.Filter(L1, L2, radius, dataType);
                    Snapshot L1Prime = DEBUGTASK3.Result;

                    TrackableTask<Snapshot> DEBUGTAS4 = PointerFilter.Filter(L2, L3, radius, dataType);
                    Snapshot L2Prime = DEBUGTAS4.Result;

                    TrackableTask<Snapshot> DEBUGTAS24 = PointerFilter.Filter(L3, snapshotDestination, radius, dataType);
                    Snapshot L3Prime = DEBUGTAS24.Result;

                    TrackableTask<Snapshot> REVALIDATION1 = PointerFilter.Filter(L1Prime, L2Prime, radius, dataType);
                    Snapshot L1PrimeCheck = REVALIDATION1.Result;

                    TrackableTask<Snapshot> REVALIDATION2 = PointerFilter.Filter(L2Prime, L3Prime, radius, dataType);
                    Snapshot L2PrimeCheck = REVALIDATION2.Result;

                    TrackableTask<Snapshot> REVALIDATION3 = PointerFilter.Filter(L3Prime, snapshotDestination, radius, dataType);
                    Snapshot L3PrimeCheck = REVALIDATION3.Result;
                    */

                    // Step 1) Back trace (we do not care about static/heap at this point)
                    foreach (Int32 currentDepth in depthRange)
                    {
                        // For the last depth, start from the static base
                        if (currentDepth == depthRange.Last())
                        {
                            currentSourceSnapshot = snapshotStatic;
                        }

                        TrackableTask<Snapshot> filterTask = PointerFilter.Filter(currentSourceSnapshot, currentTargetSnapshot, radius, dataType);
                        Snapshot pointers = filterTask.Result;

                        if (pointers.ByteCount <= 0)
                        {
                            break;
                        }

                        backTraceLevels.Add(pointers);
                        currentTargetSnapshot = pointers;
                    }

                    // Step 2) Perform a front trace on the back-trace list
                    if (backTraceLevels.Count > 0)
                    {
                        Snapshot[] normalizedLevels = backTraceLevels.Reverse().ToArray();

                        for (Int32 index = 1; index < normalizedLevels.Length; index++)
                        {
                            TrackableTask<Snapshot> filterTask = PointerFilter.Filter(normalizedLevels[index - 1], normalizedLevels[index], radius, dataType);
                            Snapshot pointers = filterTask.Result;

                            levels.Add(pointers);
                        }
                    }

                    levels.Add(snapshotDestination);

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