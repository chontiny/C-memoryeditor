namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Source.Scanners.Pointers.Discovered;
    using SqualrCore.Source.ActionScheduler;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Implements an algorithm which finds all possible paths between pointer levels, once they are discovered by the back trace algorithm.
    /// </summary>
    internal class PointerScan : ScheduledTask
    {
        /// <summary>
        /// Creates an instance of the <see cref="PointerScan" /> class.
        /// </summary>
        public PointerScan(UInt64 targetAddress) : base(
            taskName: "Pointer Scan",
            isRepeated: false,
            trackProgress: true)
        {
            this.AccessLock = new Object();

            this.TargetAddress = targetAddress;

            this.Dependencies.Enqueue(new PointerBackTracer(targetAddress, this.SetLevelPointers));
        }

        private Stack<ConcurrentDictionary<UInt64, UInt64>> LevelPointers { get; set; }

        private UInt64 TargetAddress { get; set; }

        private Object AccessLock { get; set; }

        protected override void OnBegin()
        {
            if (this.LevelPointers == null || this.LevelPointers.Count <= 0)
            {
                this.Cancel();
            }
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            IList<PointerTree> pointerTrees = new List<PointerTree>();

            cancellationToken.ThrowIfCancellationRequested();

            // Collect base addresses
            ConcurrentDictionary<UInt64, UInt64> modulePointers = this.LevelPointers.Pop();

            foreach (KeyValuePair<UInt64, UInt64> modulePointer in modulePointers)
            {
                pointerTrees.Add(new PointerTree(modulePointer.Key));
            }

            ConcurrentDictionary<UInt64, UInt64> currentPointers = modulePointers;
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.LevelPointers = null;
        }

        private void SetLevelPointers(Stack<ConcurrentDictionary<UInt64, UInt64>> levelSnapshots)
        {
            this.LevelPointers = levelSnapshots;
        }
    }
    //// End class
}
//// End namespace