namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Source.Results.PointerScanResults;
    using Squalr.Source.Scanners.Pointers.Structures;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Threading;

    /// <summary>
    /// Enumerates existing discovered pointers to produce a set of validated discovered pointers based on their values.
    /// </summary>
    internal class PointerValueRescan : ScheduledTask
    {
        /// <summary>
        /// Creates an instance of the <see cref="PointerValueRescan" /> class.
        /// </summary>
        /// <param name="targetAddress">The target address of the poitner scan.</param>
        public PointerValueRescan() : base(
            taskName: "Pointer Value Rescan",
            isRepeated: false,
            trackProgress: true)
        {
            this.ProgressLock = new Object();
        }

        /// <summary>
        /// Gets or sets the target address of the pointer scan.
        /// </summary>
        public Object Value { get; set; }

        /// <summary>
        /// Gets or sets the discovered pointers from the pointer scan.
        /// </summary>
        private DiscoveredPointers DiscoveredPointers { get; set; }

        /// <summary>
        /// Gets or sets a lock object for updating scan progress.
        /// </summary>
        private Object ProgressLock { get; set; }

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
            String valueString = this.Value?.ToString() ?? String.Empty;

            // Enumerate all discovered pointers and determine if they have a valid target address
            foreach (PointerItem pointerItem in this.DiscoveredPointers)
            {
                pointerItem.Update();

                if (pointerItem.AddressValue?.ToString() == valueString)
                {
                    validatedPointers.Pointers.Add(pointerItem);
                }

                // Update scan progress
                lock (this.ProgressLock)
                {
                    processedPointers++;

                    // Limit how often we update the progress
                    if (processedPointers % 1000 == 0)
                    {
                        this.UpdateProgress(processedPointers, this.DiscoveredPointers.Count.ToInt32(), canFinalize: false);
                    }
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
