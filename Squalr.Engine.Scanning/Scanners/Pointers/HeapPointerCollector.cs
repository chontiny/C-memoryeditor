using Squalr.Engine.DataTypes;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collects static pointers in the target process.
    /// </summary>
    public static class HeapPointercollector
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Heap Pointer Collector";

        /// <summary>
        /// Begins the manual scan based on the provided snapshot and parameters.
        /// </summary>
        /// <returns></returns>
        public static TrackableTask<Snapshot> Collect(DataType dataType)
        {
            TrackableTask<Snapshot> trackedScanTask = new TrackableTask<Snapshot>(HeapPointercollector.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<Snapshot> scanTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                Snapshot snapshot = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    // This snapshot is just to get the bounds of the process memory for determining if a pointer is valid
                    Snapshot userModeSnapshot = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromUserModeMemory, dataType);

                    // Collect heaps
                    snapshot = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromHeap, dataType);
                    TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(snapshot);
                    snapshot = valueCollector.Result;

                    // Exit if canceled
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Heap pointer collection complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Heap pointer collection canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error performing heap pointer collection", ex);
                    return null;
                }

                return snapshot;
            }, cancellationTokenSource.Token);

            trackedScanTask.SetTrackedTask(scanTask, cancellationTokenSource);

            return trackedScanTask;
        }
    }
    //// End class
}
//// End namespace