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
        public static Task<Snapshot> CollectValues(Snapshot snapshot, DataType dataType, OnProgressUpdate onProgressUpdate, out CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            snapshot = snapshot.Clone("Value Collector");

            return Task.Factory.StartNew<Snapshot>(() =>
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
                        if (onProgressUpdate != null && Interlocked.Increment(ref processedRegions) % 32 == 0)
                        {
                            // Check for canceled scan
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            onProgressUpdate(processedRegions / snapshot.RegionCount);
                        }
                    });

                cancellationToken.ThrowIfCancellationRequested();
                stopwatch.Stop();
                Logger.Log(LogLevel.Info, "Values collected in: " + stopwatch.Elapsed);

                return snapshot;
            }, cancellationTokenSource.Token);
        }
    }
    //// End class
}
//// End namespace