namespace Squalr.Source.Scanners.Pointers
{
    using Snapshots;
    using Squalr.Properties;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Scanners.ValueCollector;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Collects all module and heap pointers in the running process.
    /// </summary>
    internal class PointerCollector : ScheduledTask
    {
        /// <summary>
        /// The rounding size for pointer destinations.
        /// </summary>
        private const Int32 PointerRoundingSize = 1024;

        /// <summary>
        /// Creates an instance of the <see cref="PointerCollector" /> class.
        /// </summary>
        public PointerCollector(Action<PointerPool, PointerPool> collectedPointersCallback) : base(
            taskName: "Pointer Collector",
            isRepeated: false,
            trackProgress: true)
        {
            this.ProgressLock = new Object();
            this.CollectedPointersCallback = collectedPointersCallback;

            this.Dependencies.Enqueue(new ValueCollectorModel(this.SetSnapshot));
        }

        /// <summary>
        /// Gets or sets the current snapshot being parsed.
        /// </summary>
        private Snapshot Snapshot { get; set; }

        /// <summary>
        /// Gets or sets the collected module pointers.
        /// </summary>
        private PointerPool ModulePointers { get; set; }

        /// <summary>
        /// Gets or sets the collected heap pointers.
        /// </summary>
        private PointerPool HeapPointers { get; set; }

        /// <summary>
        /// Gets or sets the callback function to which we pass the collected pointers.
        /// </summary>
        private Action<PointerPool, PointerPool> CollectedPointersCallback;

        /// <summary>
        /// Gets or sets a lock object for updating scan progress.
        /// </summary>
        private Object ProgressLock { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
            this.ModulePointers = new PointerPool();
            this.HeapPointers = new PointerPool();

            if (this.Snapshot == null)
            {
                this.Cancel();
            }

            Boolean isProcess32Bit = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit();
            this.Snapshot.UpdateSettings(activeType: isProcess32Bit ? typeof(UInt32) : typeof(UInt64), alignment: isProcess32Bit ? sizeof(UInt32) : sizeof(UInt64));
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Int32 processedRegions = 0;

            Boolean isProcess32Bit = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit();

            // Create the base snapshot from the loaded modules
            IEnumerable<SnapshotRegion> regions = EngineCore.GetInstance().VirtualMemory.GetModules().Select(region => new SnapshotRegion(region));
            Snapshot moduleSnapshot = new Snapshot(regions);

            // Process the allowed amount of chunks from the priority queue
            Parallel.ForEach(
                this.Snapshot.Cast<SnapshotRegion>(),
                SettingsViewModel.GetInstance().ParallelSettingsFullCpu,
                (region) =>
                {
                    if (region.CurrentValues == null || region.CurrentValues.Length <= 0)
                    {
                        return;
                    }

                    if (isProcess32Bit)
                    {
                        for (IEnumerator<SnapshotElementIterator> enumerator = region.IterateElements(PointerIncrementMode.CurrentOnly); enumerator.MoveNext();)
                        {
                            SnapshotElementIterator element = enumerator.Current;
                            UInt32 value = unchecked((UInt32)element.GetCurrentValue());

                            // Enforce 4-byte alignment of destination, and filter out small (invalid) pointers
                            if (value < UInt16.MaxValue || value % sizeof(UInt32) != 0)
                            {
                                continue;
                            }

                            // Check if it is possible that this pointer is valid, if so keep it
                            if (this.Snapshot.ContainsAddress(value))
                            {
                                if (moduleSnapshot.ContainsAddress(value))
                                {
                                    this.ModulePointers[element.BaseAddress.ToUInt64()] = value;
                                }
                                else
                                {
                                    this.HeapPointers[element.BaseAddress.ToUInt64()] = value;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (IEnumerator<SnapshotElementIterator> enumerator = region.IterateElements(PointerIncrementMode.CurrentOnly); enumerator.MoveNext();)
                        {
                            SnapshotElementIterator element = enumerator.Current;
                            UInt64 value = unchecked((UInt64)element.GetCurrentValue());

                            // Enforce 8-byte alignment of destination, and filter out small (invalid) pointers
                            if (value < UInt16.MaxValue || value % sizeof(UInt64) != 0)
                            {
                                continue;
                            }

                            // Check if it is possible that this pointer is valid, if so keep it
                            if (this.Snapshot.ContainsAddress(value))
                            {
                                if (moduleSnapshot.ContainsAddress(value))
                                {
                                    this.ModulePointers[element.BaseAddress.ToUInt64()] = value;
                                }
                                else
                                {
                                    this.HeapPointers[element.BaseAddress.ToUInt64()] = value;
                                }
                            }
                        }
                    }

                    // Clear the saved values, we do not need them now
                    region.SetCurrentValues(null);

                    // Update scan progress
                    lock (this.ProgressLock)
                    {
                        processedRegions++;

                        // Limit how often we update the progress
                        if (processedRegions % 10 == 0)
                        {
                            this.UpdateProgress(processedRegions, this.Snapshot.RegionCount, canFinalize: false);
                        }
                    }
                });
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.CollectedPointersCallback?.Invoke(this.ModulePointers, this.HeapPointers);

            this.UpdateProgress(ScheduledTask.MaximumProgress);

            this.Snapshot = null;
            this.ModulePointers = null;
            this.HeapPointers = null;
        }

        /// <summary>
        /// Sets the snapshot to scan.
        /// </summary>
        /// <param name="snapshot">The snapshot to scan.</param>
        private void SetSnapshot(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
        }
    }
    //// End class
}
//// End namespace