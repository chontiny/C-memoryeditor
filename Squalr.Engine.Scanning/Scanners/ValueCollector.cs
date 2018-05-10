namespace Squalr.Engine.Scanning.Scanners
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collect values for a given snapshot. The values are assigned to a new snapshot.
    /// </summary>
    public static class ValueCollector
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Value Collector";

        public static TrackableTask<Snapshot> CollectValues(Snapshot snapshot)
        {
            TrackableTask<Snapshot> trackableValueCollectTask = new TrackableTask<Snapshot>(ValueCollector.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            snapshot = snapshot.Clone(ValueCollector.Name);

            Task<Snapshot> valueCollectTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                try
                {
                    Int32 processedRegions = 0;

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    ParallelOptions options = ParallelSettings.ParallelSettingsFastest.Clone();
                    options.CancellationToken = cancellationTokenSource.Token;

                    // Read memory to get current values for each region
                    Parallel.ForEach(
                        snapshot.OptimizedReadGroups,
                        options,
                        (readGroup) =>
                        {
                            // Check for canceled scan
                            cancellationTokenSource.Token.ThrowIfCancellationRequested();

                            // Read the memory for this region
                            readGroup.ReadAllMemory();

                            // Update progress every N regions
                            if (Interlocked.Increment(ref processedRegions) % 32 == 0)
                            {
                                trackableValueCollectTask.Progress = (float)processedRegions / (float)snapshot.RegionCount * 100.0f;
                            }
                        });

                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Values collected in: " + stopwatch.Elapsed);
                    return snapshot;
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Scan canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error performing scan", ex);
                    return null;
                }
            }, cancellationTokenSource.Token);

            trackableValueCollectTask.SetTrackedTask(valueCollectTask, cancellationTokenSource);

            return trackableValueCollectTask;
        }
    }
    //// End class
}
//// End namespace