namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Snapshots;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Threading;

    /// <summary>
    /// Enumerates existing discovered pointers to produce a set of validated discovered pointers.
    /// </summary>
    public class PointerValidationScan : ScheduledTask
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
            throw new NotImplementedException(); ////this.DiscoveredPointers = PointerScanResultsViewModel.GetInstance().DiscoveredPointers;
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

            Snapshot snapshot = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromUserModeMemory);

            // Enumerate all discovered pointers and determine if they have a valid target address
            foreach (Pointer pointer in this.DiscoveredPointers)
            {
                pointer.Update();

                if (pointer.CalculatedAddress != IntPtr.Zero && snapshot.ContainsAddress(pointer.CalculatedAddress.ToUInt64()))
                {
                    validatedPointers.Pointers.Add(pointer);
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
            throw new NotImplementedException(); ////PointerScanResultsViewModel.GetInstance().DiscoveredPointers = this.DiscoveredPointers;
        }
    }
    //// End class
}
//// End namespace
