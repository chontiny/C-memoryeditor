namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Engine.TaskScheduler;
    using Squalr.Source.ProjectItems;
    using Squalr.Source.Results;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Snapshots;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Threading;

    /// <summary>
    /// Enumerates existing discovered pointers to produce a set of validated discovered pointers.
    /// </summary>
    internal class PointerValidationScan : ScheduledTask
    {
        /// <summary>
        /// Creates an instance of the <see cref="PointerValidationScan" /> class.
        /// </summary>
        /// <param name="targetAddress">The target address of the poitner scan.</param>
        public PointerValidationScan() : base(
            taskName: "Pointer Validation",
            isRepeated: false,
            trackProgress: true)
        {
        }

        /// <summary>
        /// Gets or sets the discovered pointers from the pointer scan.
        /// </summary>
        private DiscoveredPointers DiscoveredPointers { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
            this.DiscoveredPointers = PointerScanResultsViewModel.GetInstance().DiscoveredPointers;
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ValidatedPointers validatedPointers = new ValidatedPointers();
            Int32 processedPointers = 0;

            Snapshot snapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromUserModeMemory);

            // Enumerate all discovered pointers and determine if they have a valid target address
            foreach (PointerItem pointerItem in this.DiscoveredPointers)
            {
                pointerItem.Update();

                if (pointerItem.CalculatedAddress != IntPtr.Zero && snapshot.ContainsAddress(pointerItem.CalculatedAddress.ToUInt64()))
                {
                    validatedPointers.Pointers.Add(pointerItem);
                }

                // Update scan progress
                if (Interlocked.Increment(ref processedPointers) % 1024 == 0)
                {
                    this.UpdateProgress(processedPointers, this.DiscoveredPointers.Count.ToInt32(), canFinalize: false);
                }
            }

            this.DiscoveredPointers = validatedPointers;
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            PointerScanResultsViewModel.GetInstance().DiscoveredPointers = this.DiscoveredPointers;
        }
    }
    //// End class
}
//// End namespace
