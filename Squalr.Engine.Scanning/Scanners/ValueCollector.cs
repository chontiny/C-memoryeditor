namespace Squalr.Engine.Scanning.Scanners
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Snapshots;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collect values for a given snapshot. The values are assigned to a new snapshot.
    /// </summary>
    public static class ValueCollector
    {
        public static TrackableTask<Snapshot> CollectValues(Snapshot snapshot, DataType dataType)
        {
            TrackableTask<Snapshot> trackableValueCollectTask = new TrackableTask<Snapshot>("Value Collector");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            snapshot = snapshot.Clone("Value Collector");

            Task<Snapshot> valueCollectTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                Int32 processedRegions = 0;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Read memory to get current values for each region
                Parallel.ForEach(
                    snapshot.OptimizedReadGroups,
                    ParallelSettings.ParallelSettingsFastest,
                    (readGroup) =>
                    {
                        // Read the memory for this region
                        readGroup.ReadAllMemory();

                        // Update progress every N regions
                        if (Interlocked.Increment(ref processedRegions) % 32 == 0)
                        {
                            // Check for canceled scan
                            if (cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                return;
                            }

                            trackableValueCollectTask.UpdateProgress((float)processedRegions / (float)snapshot.RegionCount * 100.0f);
                        }
                    });

                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                stopwatch.Stop();
                Logger.Log(LogLevel.Info, "Values collected in: " + stopwatch.Elapsed);

                return snapshot;
            }, cancellationTokenSource.Token);

            trackableValueCollectTask.SetTrackedTask(valueCollectTask, cancellationTokenSource);

            return trackableValueCollectTask;
        }
    }
    //// End class
}
//// End namespace