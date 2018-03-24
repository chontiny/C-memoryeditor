namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Engine.TaskScheduler;
    using Squalr.Source.ProjectItems;
    using Squalr.Source.Results;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Utils.Extensions;
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
            Int64 processedPointers = 0;

            // Enumerate all discovered pointers and determine if they have a valid target address
            foreach (PointerItem pointerItem in this.DiscoveredPointers)
            {
                pointerItem.Update();

                // TODO: This is not particularly sustainable/performant. This will fall apart for floats/doubles where we want nearly-equal values (ie 3.4444 and 3.4443)
                // Ideally we want something similar to how we do scans with the SnapShotElementIterator with the call to Compare().
                // One solution would be to create a snapshot from all of the discovered pointers so that we could leverage the SnapshotElementIterator compare functions,
                // But this creates the technical challenge of associating the pointer item with an element in the snapshot.
                // Also, we need to update the data type of these pointer items to match the current pointer scan results data times.
                if (pointerItem.AddressValue?.Equals(this.Value) ?? false)
                {
                    validatedPointers.Pointers.Add(pointerItem);
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
            PointerScanResultsViewModel.GetInstance().DiscoveredPointers = this.DiscoveredPointers;
        }
    }
    //// End class
}
//// End namespace
