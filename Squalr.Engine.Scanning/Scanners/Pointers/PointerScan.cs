using Squalr.Engine.DataTypes;
using Squalr.Engine.OS;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Scans for pointers in the target process.
    /// </summary>
    public static class PointerScan
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Pointer Scan";

        /// <summary>
        /// Performs a pointer scan for a given address.
        /// </summary>
        /// <param name="address">The address for which to perform a pointer scan.</param>
        /// <param name="radius">The maximum pointer search depth.</param>
        /// <param name="depth">The maximum pointer search depth.</param>
        /// <param name="alignment">The pointer scan alignment.</param>
        /// <returns>Atrackable task that returns the scan results.</returns>
        public static TrackableTask<Snapshot> Scan(UInt64 address, UInt32 radius, Int32 depth, Int32 alignment)
        {
            TrackableTask<Snapshot> trackedScanTask = new TrackableTask<Snapshot>(PointerScan.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Boolean isProcess32Bit = Processes.Default.IsOpenedProcess32Bit();
            DataType dataType = isProcess32Bit ? DataType.UInt32 : DataType.UInt64;
            Int32 size = isProcess32Bit ? 4 : 8;
            Int32 vectorSize = Vectors.VectorSize;

            Task<Snapshot> scanTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                Snapshot result = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Snapshot userModeMemory = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromUserModeMemory, dataType);

                    // Step 1) Create a snapshot of the target address
                    Snapshot targetAddress = new Snapshot(new SnapshotRegion[] { new SnapshotRegion(new ReadGroup(address, size, dataType, alignment), 0, size) });

                    // Step 2) Collect static pointers
                    Snapshot staticPointers = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromModules, dataType);
                    TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(staticPointers);
                    staticPointers = valueCollector.Result;
                    //  TrackableTask<Snapshot> staticPointerCollectorTask = StaticPointercollector.Collect(dataType);
                    // Snapshot staticPointers = staticPointerCollectorTask.Result;

                    // Step 3) Build levels
                    TrackableTask<IList<Snapshot>> levelBuilderTask = LevelBuilder.Build(staticPointers, targetAddress, depth, radius, dataType);
                    IList<Snapshot> levels = levelBuilderTask.Result;

                    PointerCollection collection = new PointerCollection(levels, radius, dataType);

                    Pointer debug = collection.GetRandomPointer();
                    Pointer debug2 = collection.GetRandomPointer();
                    Pointer debug3 = collection.GetRandomPointer();
                    Pointer debug4 = collection.GetRandomPointer();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Pointer scan complete in: " + stopwatch.Elapsed);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Pointer scan canceled", ex);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error performing pointer scan", ex);
                }

                return result;
            }, cancellationTokenSource.Token);

            trackedScanTask.SetTrackedTask(scanTask, cancellationTokenSource);

            return trackedScanTask;
        }
    }
    //// End class
}
//// End namespace