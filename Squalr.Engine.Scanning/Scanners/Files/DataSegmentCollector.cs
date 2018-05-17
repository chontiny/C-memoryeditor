namespace Squalr.Engine.Scanning.Scanners
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Memory;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collect data segment values for a given snapshot
    /// </summary>
    public static class DataSegmentCollector
    {
        /// <summary>
        /// The name of this scan.
        /// </summary>
        private const String Name = "Data Segment Collector";

        public static TrackableTask<Snapshot> CollectDataSegments(Boolean ignoreSystemModules = true)
        {
            TrackableTask<Snapshot> trackableValueCollectTask = new TrackableTask<Snapshot>(DataSegmentCollector.Name);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<Snapshot> dataSegmentCollectorTask = Task.Factory.StartNew<Snapshot>(() =>
            {
                try
                {
                    Int32 processedModules = 0;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    // Construct a snapshot from static data segments in target process modules
                    IEnumerable<NormalizedModule> modules = Query.Default.GetModules();
                    DirectoryInfo systemPathInfo = new DirectoryInfo(Environment.SystemDirectory);
                    ConcurrentBag<IList<SnapshotRegion>> dataRegions = new ConcurrentBag<IList<SnapshotRegion>>();

                    Parallel.ForEach(
                        modules,
                        ParallelSettings.ParallelSettingsFastest,
                        (module) =>
                        {
                            DirectoryInfo modulePathInfo = new DirectoryInfo(module.FullPath);

                            // Collect module data segments, unless it is a system module and ignore is set
                            if (!ignoreSystemModules || modulePathInfo.Parent != systemPathInfo)
                            {
                                IList<SnapshotRegion> dataSegments = MetaData.Default.GetDataSegments(module.BaseAddress, module.FullPath);
                                dataRegions.Add(dataSegments);
                            }

                            // Update progress every N regions
                            if (Interlocked.Increment(ref processedModules) % 32 == 0)
                            {
                                trackableValueCollectTask.Progress = (float)processedModules / (float)modules.Count() * 100.0f;
                            }
                        });

                    Snapshot result = new Snapshot(DataSegmentCollector.Name, dataRegions.SelectMany(region => region));
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    stopwatch.Stop();
                    Logger.Log(LogLevel.Info, "Data segments collected in: " + stopwatch.Elapsed);
                    return result;
                }
                catch (OperationCanceledException ex)
                {
                    Logger.Log(LogLevel.Warn, "Data segment collection canceled", ex);
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error performing data segment collection", ex);
                    return null;
                }
            }, cancellationTokenSource.Token);

            trackableValueCollectTask.SetTrackedTask(dataSegmentCollectorTask, cancellationTokenSource);

            return trackableValueCollectTask;
        }
    }
    //// End class
}
//// End namespace