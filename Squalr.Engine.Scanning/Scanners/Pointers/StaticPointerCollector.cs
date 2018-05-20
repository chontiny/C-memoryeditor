using Squalr.Engine.DataTypes;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Scanners.Constraints;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using static Squalr.Engine.TrackableTask;

    /// <summary>
    /// Collects static pointers in the target process.
    /// </summary>
    public static class StaticPointerCollector
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Static Pointer Collector";

        /// <summary>
        /// Begins the manual scan based on the provided snapshot and parameters.
        /// </summary>
        /// <returns></returns>
        public static TrackableTask<Snapshot> Collect(DataType dataType)
        {
            return TrackableTask<Snapshot>
                .Create(StaticPointerCollector.Name, out UpdateProgress updateProgress, out CancellationToken cancellationToken)
                .With(Task.Factory.StartNew<Snapshot>(() =>
                {
                    Snapshot snapshot = null;

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        // This snapshot is just to get the bounds of the process memory for determining if a pointer is valid
                        Snapshot userModeSnapshot = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromUserModeMemory, dataType);

                        // Collect static data segments from the binary file header
                        TrackableTask<Snapshot> dataSegmentCollector = DataSegmentCollector.CollectDataSegments(ignoreSystemModules: true);
                        snapshot = dataSegmentCollector.Result;

                        // Collect the in-memory values so we can perform a diff
                        TrackableTask<Snapshot> valueCollector = ValueCollector.CollectValues(snapshot);
                        snapshot = valueCollector.Result;

                        // Perform a changed value scan against the data segments and the in-memory values
                        TrackableTask<Snapshot> changedScan = ManualScanner.Scan(snapshot, new ScanConstraint(ScanConstraint.ConstraintType.Changed));
                        snapshot = changedScan.Result;
                        snapshot.ElementDataType = dataType;

                        stopwatch.Stop();
                        Logger.Log(LogLevel.Info, "Static pointer collection complete in: " + stopwatch.Elapsed);
                    }
                    catch (OperationCanceledException ex)
                    {
                        Logger.Log(LogLevel.Warn, "Static pointer collection canceled", ex);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error performing static pointer collection", ex);
                        return null;
                    }

                    return snapshot;
                }, cancellationToken));
        }
    }
    //// End class
}
//// End namespace