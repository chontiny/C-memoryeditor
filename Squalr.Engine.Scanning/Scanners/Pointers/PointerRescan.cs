namespace Squalr.Engine.Scanning.Scanners.Pointers
{
    using Squalr.Engine.Scanning.Scanners.Pointers.Structures;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Threading;

    /// <summary>
    /// Enumerates existing discovered pointers to produce a set of validated discovered pointers.
    /// </summary>
    public class PointerRescan : ScheduledTask
    {
        /// <summary>
        /// Creates an instance of the <see cref="PointerRescan" /> class.
        /// </summary>
        /// <param name="targetAddress">The target address of the poitner scan.</param>
        public PointerRescan() : base(
            taskName: "Pointer Rescan",
            isRepeated: false,
            trackProgress: true)
        {
        }

        /// <summary>
        /// Gets or sets the target address of the pointer scan.
        /// </summary>
        public UInt64 TargetAddress { get; set; }

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
            Int64 processedPointers = 0;

            // Enumerate all discovered pointers and determine if they have a valid target address
            foreach (Pointer pointer in this.DiscoveredPointers)
            {
                pointer.Update();

                if (pointer.CalculatedAddress.ToUInt64() == this.TargetAddress)
                {
                    validatedPointers.Pointers.Add(pointer);
                }

                // Update scan progress
                if (Interlocked.Increment(ref processedPointers) % 1024 == 0)
                {
                    this.UpdateProgress(processedPointers, this.DiscoveredPointers.Count.ToInt64(), canFinalize: false);
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
