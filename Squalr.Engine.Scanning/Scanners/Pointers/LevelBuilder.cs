namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels;
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
        public static TrackableTask<IList<Snapshot>> Build(Snapshot snapshotStatic, Snapshot snapshotHeaps, Snapshot snapshotDestination, Int32 depth, UInt32 radius, DataType dataType)
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
                    Snapshot currentTargetSnapshot = snapshotDestination;
                    Snapshot currentSourceSnapshot = snapshotHeaps;

                    backTraceLevels.Add(snapshotDestination);
                    IEnumerable<Int32> depthRange = Enumerable.Range(0, depth);

                    // Step 1) Back trace (we do not care about static/heap at this point)
                    foreach (Int32 currentDepth in depthRange)
                    {
                        // For the last depth, start from the static base
                        if (currentDepth == depthRange.Last())
                        {
                            currentSourceSnapshot = snapshotStatic;
                        }

                        ISearchKernel searchKernel = SearchKernelFactory.GetSearchKernel(currentTargetSnapshot, radius);
                        TrackableTask<Snapshot> filterTask = PointerFilter.Filter(currentSourceSnapshot, searchKernel, currentTargetSnapshot, radius);
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
                            ISearchKernel searchKernel = SearchKernelFactory.GetSearchKernel(normalizedLevels[index], radius);
                            TrackableTask<Snapshot> filterTask = PointerFilter.Filter(normalizedLevels[index - 1], searchKernel, normalizedLevels[index], radius);
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