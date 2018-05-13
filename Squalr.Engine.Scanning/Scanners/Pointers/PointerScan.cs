using Squalr.Engine.DataTypes;
using Squalr.Engine.OS;

namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
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
        /// Begins the manual scan based on the provided snapshot and parameters.
        /// </summary>
        /// <param name="snapshot">The snapshot on which to perfrom the scan.</param>
        /// <param name="scanConstraintCollection">The collection of scan constraints to use in the manual scan.</param>
        /// <param name="onProgressUpdate">The progress update callback.</param>
        /// <param name="cancellationTokenSource">A token for canceling the scan.</param>
        /// <returns></returns>
        public static TrackableTask<Snapshot> Scan(UInt64 address)
        {
            TrackableTask<Snapshot> trackedScanTask = new TrackableTask<Snapshot>(PointerScan.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Boolean isProcess32Bit = Processes.Default.IsOpenedProcess32Bit();
            DataType dataType;

            if (isProcess32Bit)
            {
                dataType = DataType.UInt32;
            }
            else
            {
                dataType = DataType.UInt64;
            }

            Task<Snapshot> scanTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                Snapshot result = null;

                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    // Step 1) Collect static pointers
                    TrackableTask<Snapshot> staticPointerCollectorTask = StaticPointercollector.Collect(dataType);
                    Snapshot staticPointers = staticPointerCollectorTask.Result;

                    // Step 2) Collect heap pointers
                    TrackableTask<Snapshot> heapPointerCollectorTask = HeapPointercollector.Collect(dataType);
                    Snapshot heapPointers = heapPointerCollectorTask.Result;

                    // Step 3) Build levels

                    // Step 4) Build pointer trees

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