namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Source.Scanners.Pointers.Discovered;
    using Squalr.Source.Scanners.Pointers.Structures;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Implements an algorithm which finds all possible paths between pointer levels, once they are discovered by the back trace algorithm.
    /// </summary>
    internal class PointerScan : ScheduledTask
    {
        /// <summary>
        /// Creates an instance of the <see cref="PointerScan" /> class.
        /// </summary>
        /// <param name="targetAddress">The target address of the poitner scan.</param>
        public PointerScan(UInt64 targetAddress) : base(
            taskName: "Pointer Scan",
            isRepeated: false,
            trackProgress: true)
        {
            this.ProgressLock = new Object();

            this.TargetAddress = targetAddress;
            this.PointerDepth = 1;
            this.PointerRadius = 1024;

            this.Dependencies.Enqueue(new PointerBackTracer(targetAddress, this.PointerDepth, this.PointerRadius, this.SetLevelPointers));
        }

        /// <summary>
        /// Gets or sets the collection of pointers at each depth.
        /// </summary>
        private LevelPointers LevelPointers { get; set; }

        /// <summary>
        /// Gets or sets the pointer depth of the scan.
        /// </summary>
        private UInt32 PointerDepth { get; set; }

        /// <summary>
        /// Gets or sets the pointer radius of the scan.
        /// </summary>
        private UInt32 PointerRadius { get; set; }

        /// <summary>
        /// Gets or sets the target address of the pointer scan.
        /// </summary>
        private UInt64 TargetAddress { get; set; }

        /// <summary>
        /// Gets or sets a lock object for updating scan progress.
        /// </summary>
        private Object ProgressLock { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
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
            IList<PointerRoot> pointerRoots = new List<PointerRoot>();
            PointerPool previousPointers = this.LevelPointers.ModulePointers;

            cancellationToken.ThrowIfCancellationRequested();

            // Collect pointer roots
            foreach (KeyValuePair<UInt64, UInt64> modulePointer in this.LevelPointers.ModulePointers)
            {
                pointerRoots.Add(new PointerRoot(modulePointer.Key));
            }

            foreach (PointerPool currentPointers in this.LevelPointers.HeapPointerLevels)
            {
                // Build branches for this level
                foreach (PointerRoot pointerRoot in pointerRoots)
                {
                    // First iteration (starting from modules)
                    if (previousPointers == this.LevelPointers.ModulePointers)
                    {
                        UInt64 pointerDestination = previousPointers[pointerRoot.BaseAddress];

                        IEnumerable<Int32> offsets = currentPointers
                            .PointerAddresses
                            .Select(x => x)
                            .Where(x => (x > pointerDestination - this.PointerRadius) && (x < pointerDestination + this.PointerRadius))
                            .Select(x => (x > pointerDestination) ? (x - pointerDestination).ToInt32() : -((pointerDestination - x).ToInt32()));

                        pointerRoot.AddOffsets(offsets);
                    }
                    else
                    {
                        // Now iterate the branches
                        foreach (PointerBranch pointerBranch in pointerRoot)
                        {
                            UInt64 moduleBase = this.LevelPointers.ModulePointers[pointerRoot.BaseAddress];
                            UInt64 pointerDestination =
                                (pointerBranch.Offset < 0
                                ? previousPointers[moduleBase - unchecked((UInt32)(-pointerBranch.Offset))]
                                : previousPointers[moduleBase + unchecked((UInt32)(pointerBranch.Offset))]);

                            IEnumerable<Int32> offsets = currentPointers
                                .PointerAddresses
                                .Select(x => x)
                                .Where(x => (x > pointerDestination - this.PointerRadius) && (x < pointerDestination + this.PointerRadius))
                                .Select(x => (x > pointerDestination) ? (x - pointerDestination).ToInt32() : -((pointerDestination - x).ToInt32()));

                            pointerBranch.AddOffsets(offsets);
                        }
                    }
                }

                previousPointers = currentPointers;
            }
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            this.LevelPointers = null;
        }

        private void SetLevelPointers(LevelPointers levelSnapshots)
        {
            this.LevelPointers = levelSnapshots;
        }
    }
    //// End class
}
//// End namespace