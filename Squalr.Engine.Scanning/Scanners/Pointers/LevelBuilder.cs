namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Diagnostics;
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
        /// <param name="snapshotFrom">The snapshot on which to perfrom the scan.</param>
        /// <param name="snapshotTo">The snapshot on which to perfrom the scan.</param>
        /// <returns></returns>
        public static TrackableTask<Snapshot> Build(Snapshot snapshotFrom, Snapshot snapshotTo)
        {
            TrackableTask<Snapshot> trackedScanTask = new TrackableTask<Snapshot>(LevelBuilder.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<Snapshot> filterTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                Snapshot result = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

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

                return result;
            }, cancellationTokenSource.Token);

            trackedScanTask.SetTrackedTask(filterTask, cancellationTokenSource);

            return trackedScanTask;
        }
    }
    //// End class
}
//// End namespace