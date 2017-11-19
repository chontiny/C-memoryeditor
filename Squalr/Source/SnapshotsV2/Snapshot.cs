namespace Squalr.Source.SnapshotsV2
{
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Snapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName = null)
        {
            this.SnapshotName = snapshotName == null ? String.Empty : snapshotName;
            this.MemoryRegions = new List<MemoryRegion>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        public Snapshot(params MemoryRegion[] snapshotRegions) : this(null, snapshotRegions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        public Snapshot(IEnumerable<MemoryRegion> snapshotRegions) : this(null, snapshotRegions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName, IEnumerable<MemoryRegion> snapshotRegions) : this(snapshotName, snapshotRegions?.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName = null, params MemoryRegion[] snapshotRegions) : this(snapshotName)
        {
            this.AddSnapshotRegions(snapshotRegions);
        }

        /// <summary>
        /// Gets the name associated with the method by which this snapshot was generated.
        /// </summary>
        public String SnapshotName { get; private set; }

        /// <summary>
        /// Gets the number of regions contained in this snapshot.
        /// </summary>
        /// <returns>The number of regions contained in this snapshot.</returns>
        public Int32 RegionCount
        {
            get
            {
                return this.MemoryRegions?.Count ?? 0;
            }
        }

        /// <summary>
        /// Gets the total number of bytes contained in this snapshot.
        /// </summary>
        public UInt64 ByteCount
        {
            get
            {
                return this.MemoryRegions?.Sum(x => x.ByteCount) ?? 0UL;
            }
        }

        /// <summary>
        /// Gets the number of individual elements contained in this snapshot.
        /// </summary>
        /// <returns>The number of individual elements contained in this snapshot.</returns>
        public UInt64 ElementCount
        {
            get
            {
                return this.MemoryRegions?.Sum(region => region.ElementCount) ?? 0UL;
            }
        }

        /// <summary>
        /// Gets or sets the memory regions contained in this snapshot.
        /// </summary>
        private IList<MemoryRegion> MemoryRegions { get; set; }

        /// <summary>
        /// Adds memory regions to the regions contained in this snapshot. Will automatically merge and sort regions.
        /// </summary>
        /// <param name="memoryRegions">The memory regions to add.</param>
        public void AddSnapshotRegions(params MemoryRegion[] memoryRegions)
        {
            if (this.MemoryRegions == null)
            {
                this.MemoryRegions = memoryRegions.ToList();
            }
            else
            {
                memoryRegions?.ForEach(x => this.MemoryRegions.Add(x));
            }

            //// this.MergeAndSortRegions();
        }
    }
    //// End class
}
//// End namespace