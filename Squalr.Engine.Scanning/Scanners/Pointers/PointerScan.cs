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
    using static Squalr.Engine.TrackableTask;

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
        public static TrackableTask<PointerBag> Scan(UInt64 address, UInt32 radius, Int32 depth, Int32 alignment)
        {
            TrackableTask<PointerBag> pointerScanTask = TrackableTask<PointerBag>.Create(PointerScan.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken);

            return pointerScanTask.With(Task.Factory.StartNew<PointerBag>(() =>
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Boolean isProcess32Bit = Processes.Default.IsOpenedProcess32Bit();
                        DataType dataType = isProcess32Bit ? DataType.UInt32 : DataType.UInt64;
                        Int32 size = isProcess32Bit ? 4 : 8;
                        Int32 vectorSize = Vectors.VectorSize;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        // Step 1) Create a snapshot of the target address
                        Snapshot targetAddress = new Snapshot(new SnapshotRegion[] { new SnapshotRegion(new ReadGroup(address, size, dataType, alignment), 0, size) });

                        // Step 2) Collect static pointers
                        Snapshot staticPointers = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromModules, dataType);
                        TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(staticPointers);
                        staticPointers = valueCollector.Result;

                        // Step 3) Collect heap pointers
                        Snapshot snapshotHeaps = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromHeaps, dataType);
                        TrackableTask<Snapshot> heapValueCollector = ValueCollector.CollectValues(snapshotHeaps);
                        snapshotHeaps = heapValueCollector.Result;

                        // Step 4) Build levels
                        TrackableTask<IList<Level>> levelBuilderTask = LevelBuilder.Build(pointerScanTask, staticPointers, snapshotHeaps, targetAddress, depth, radius, dataType);
                        IList<Level> levels = levelBuilderTask.Result;

                        // Step 5) Store results in a bag from which pointers can be retrieved
                        PointerBag pointerBag = new PointerBag(levels, radius, dataType);

                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Pointer scan complete in: " + stopwatch.Elapsed);

                        return pointerBag;
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Pointer scan canceled", ex);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error performing pointer scan", ex);
                    }

                    return null;
                }, cancellationToken));
        }
    }
    //// End class
}
//// End namespace