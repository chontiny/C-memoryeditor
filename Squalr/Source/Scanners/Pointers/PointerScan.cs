namespace Squalr.Source.Scanners.Pointers
{
    using Squalr.Engine.TaskScheduler;
    using Squalr.Source.Results;
    using Squalr.Source.Scanners.Pointers.Structures;
    using System;
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
        public PointerScan() : base(
            taskName: "Pointer Scan",
            isRepeated: false,
            trackProgress: true)
        {
            this.PointerBackTracer = new PointerBackTracer(this.SetLevelPointers);

            this.PointerDepth = 1;
            this.PointerRadius = 1024;

            this.Dependencies.Enqueue(PointerBackTracer);
        }

        /// <summary>
        /// Gets or sets the pointer back tracer task.
        /// </summary>
        private PointerBackTracer PointerBackTracer { get; set; }

        /// <summary>
        /// Gets or sets the collection of pointers at each depth.
        /// </summary>
        private LevelPointers LevelPointers { get; set; }

        /// <summary>
        /// Gets or sets the target address of the pointer scan.
        /// </summary>
        public UInt64 TargetAddress
        {
            get
            {
                return this.PointerBackTracer.TargetAddress;
            }

            set
            {
                this.PointerBackTracer.TargetAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the pointer depth of the scan.
        /// </summary>
        public Int32 PointerDepth
        {
            get
            {
                return this.PointerBackTracer.PointerDepth;
            }

            set
            {
                this.PointerBackTracer.PointerDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the pointer radius of the scan.
        /// </summary>
        public Int32 PointerRadius
        {
            get
            {
                return this.PointerBackTracer.PointerRadius;
            }

            set
            {
                this.PointerBackTracer.PointerRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the discovered pointers from the pointer scan.
        /// </summary>
        private ScannedPointers ScannedPointers { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
            this.ScannedPointers = new ScannedPointers();

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
            cancellationToken.ThrowIfCancellationRequested();
            Int32 processedPointerRoots = 0;

            // We start from modules and build paths to the destination. Create the roots from our found module pointers.
            this.ScannedPointers.CreatePointerRoots(this.LevelPointers.ModulePointerPool.PointerAddresses);

            // Build out pointer paths via a DFS
            foreach (PointerRoot pointerRoot in this.ScannedPointers.PointerRoots)
            {
                cancellationToken.ThrowIfCancellationRequested();

                PointerPool nextLevel = this.LevelPointers.DynamicPointerPools.First();
                UInt64 pointerDestination = this.LevelPointers.ModulePointerPool[pointerRoot.BaseAddress];

                pointerRoot.AddOffsets(nextLevel.FindOffsets(pointerDestination, this.PointerRadius));

                // Recurse on the branches
                if (this.LevelPointers.IsMultiLevel)
                {
                    foreach (PointerBranch branch in pointerRoot)
                    {
                        this.BuildPointerPaths(this.ApplyOffset(pointerDestination, branch.Offset), branch);
                    }
                }

                // Update scan progress
                if (Interlocked.Increment(ref processedPointerRoots) % 32 == 0)
                {
                    this.UpdateProgress(processedPointerRoots, this.ScannedPointers.PointerRoots.Count(), canFinalize: false);
                }
            }

            this.ScannedPointers.BuildCount();
        }

        private void BuildPointerPaths(UInt64 currentPointer, PointerBranch pointerBranch, Int32 levelIndex = 0)
        {
            PointerPool currentLevel = this.LevelPointers.DynamicPointerPools.ElementAt(levelIndex);
            PointerPool nextLevel = this.LevelPointers.DynamicPointerPools.ElementAt(levelIndex + 1);
            UInt64 pointerDestination = currentLevel[currentPointer];

            pointerBranch.AddOffsets(nextLevel.FindOffsets(pointerDestination, this.PointerRadius));

            // Stop recursing if no more levels
            if (levelIndex + 1 >= this.LevelPointers.DynamicPointerPools.Count() - 1)
            {
                return;
            }

            foreach (PointerBranch branch in pointerBranch)
            {
                this.BuildPointerPaths(this.ApplyOffset(pointerDestination, branch.Offset), branch, levelIndex + 1);
            }
        }

        private UInt64 ApplyOffset(UInt64 address, Int32 offset)
        {
            return (offset < 0 ? address - unchecked((UInt32)(-offset)) : address + unchecked((UInt32)(offset)));
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
            PointerScanResultsViewModel.GetInstance().DiscoveredPointers = this.ScannedPointers;

            this.LevelPointers = null;
            this.ScannedPointers = null;
        }

        private void SetLevelPointers(LevelPointers levelPointers)
        {
            this.LevelPointers = levelPointers;
        }
    }
    //// End class
}
//// End namespace
