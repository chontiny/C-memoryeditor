namespace Squalr.Source.SnapshotsV2
{
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class Snapshot : IEnumerable<SnapshotRegion>
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
                return this.SnapshotRegions?.Count() ?? 0;
            }
        }

        /// <summary>
        /// Gets the total number of bytes contained in this snapshot.
        /// </summary>
        public UInt64 ByteCount
        {
            get
            {
                return this.SnapshotRegions?.Sum(x => x.ByteCount) ?? 0UL;
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
                return this.SnapshotRegions?.Sum(region => region.ElementCount) ?? 0UL;
            }
        }

        public DataType LabelDataType
        {
            set
            {
                this.SnapshotRegions.ForEach(region => region.LabelDataType = value);
            }
        }

        /// <summary>
        /// Gets or sets the memory regions contained in this snapshot.
        /// </summary>
        private IList<MemoryRegion> MemoryRegions { get; set; }

        private IEnumerable<SnapshotRegion> SnapshotRegions
        {
            get
            {
                IEnumerator<SnapshotRegion> enumerator = this.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
        }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index. Notes: This does NOT index into a region.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        public SnapshotElementIterator this[UInt64 index]
        {
            get
            {
                foreach (SnapshotRegion region in this)
                {
                    UInt64 elementCount = region.ElementCount;

                    if (index >= elementCount)
                    {
                        index -= elementCount;
                    }
                    else
                    {
                        return region[(Int32)index * region.Alignment];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Creates a shallow clone of this snapshot.
        /// </summary>
        /// <param name="newSnapshotName">The snapshot generation method name.</param>
        /// <returns>The shallow cloned snapshot.</returns>
        public Snapshot Clone(String newSnapshotName = null)
        {
            return new Snapshot(newSnapshotName, this.MemoryRegions);
        }

        /// <summary>
        /// Determines if an address is contained in this snapshot.
        /// </summary>
        /// <param name="address">The address for which we are searching.</param>
        /// <returns>True if the address is contained.</returns>
        public Boolean ContainsAddress(UInt64 address)
        {
            if (this[address] != null)
            {
                return true;
            }

            return false;
        }

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

        public IEnumerator<SnapshotRegion> GetEnumerator()
        {
            return this.MemoryRegions?.SelectMany(snapshotRegion => snapshotRegion).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.MemoryRegions?.SelectMany(snapshotRegion => snapshotRegion).GetEnumerator();
        }
    }
    //// End class
}
//// End namespace