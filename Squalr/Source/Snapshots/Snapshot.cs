namespace Squalr.Source.Snapshots
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
        /// <param name="memoryRegions">The regions with which to initialize this snapshot.</param>
        public Snapshot(IEnumerable<MemoryRegion> memoryRegions) : this()
        {
            this.MemoryRegions = memoryRegions?.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        public Snapshot(params SnapshotRegion[] snapshotRegions) : this(null, snapshotRegions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        public Snapshot(IEnumerable<SnapshotRegion> snapshotRegions) : this(null, snapshotRegions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName, IEnumerable<SnapshotRegion> snapshotRegions) : this(snapshotName, snapshotRegions?.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName = null, params SnapshotRegion[] snapshotRegions) : this(snapshotName)
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

        public Int32 Alignment
        {
            set
            {
                this.SnapshotRegions.ForEach(region => region.Alignment = value);
            }
        }

        public DataType ElementDataType
        {
            set
            {
                this.SnapshotRegions.ForEach(region => region.ElementDataType = value);
            }
        }

        /// <summary>
        /// Gets or sets the memory regions contained in this snapshot.
        /// </summary>
        private IList<MemoryRegion> MemoryRegions { get; set; }

        /// <summary>
        /// Gets the time since the last update was performed on this snapshot.
        /// </summary>
        public DateTime TimeSinceLastUpdate { get; private set; }

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
            return new Snapshot(newSnapshotName, this.SnapshotRegions);
        }

        /// <summary>
        /// Sets all valid bits to the specified boolean value for each memory region contained.
        /// </summary>
        /// <param name="isValid">Value indicating if valid bits will be marked as valid or invalid.</param>
        public void SetAllValidBits(Boolean isValid)
        {
            this.SnapshotRegions?.ForEach(x => x.SetAllValidBits(isValid));
        }

        /// <summary>
        /// Discards all sections of memory marked with invalid bits.
        /// </summary>
        public void DiscardInvalidRegions()
        {
            List<SnapshotRegion> validRegions = new List<SnapshotRegion>();
            this.SnapshotRegions?.ForEach(x => validRegions.AddRange(x.GetValidRegions()));

            // TODO: Something that isn't this
            // this.SnapshotRegions = validRegions;
        }

        /// <summary>
        /// Reads all memory for every region contained in this snapshot. TODO: This is not parallel, nor does it track progress.
        /// </summary>
        public void ReadAllMemory()
        {
            this.TimeSinceLastUpdate = DateTime.Now;
            this.Intersect(SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromSettings));
            this.SnapshotRegions?.ForEach(x => x.ReadAllMemory());
        }

        /// <summary>
        /// Sets the label of every element in this snapshot to the same value.
        /// </summary>
        /// <typeparam name="LabelType">The data type of the label.</typeparam>
        /// <param name="label">The new snapshot label value.</param>
        public void SetElementLabels<LabelType>(LabelType label) where LabelType : struct, IComparable<LabelType>
        {
            this.SnapshotRegions?.ForEach(x => x.SetElementLabels(Enumerable.Repeat(label, x.RegionSize.ToInt32()).Cast<Object>().ToArray()));
        }

        /// <summary>
        /// Gets the time since the last update was performed on this snapshot.
        /// </summary>
        /// <returns>The time since the last update.</returns>
        public DateTime GetTimeSinceLastUpdate()
        {
            return this.TimeSinceLastUpdate;
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
        /// Adds snapshot regions to the regions contained in this snapshot. Will automatically merge and sort regions.
        /// </summary>
        /// <param name="snapshotRegions">The snapshot regions to add.</param>
        public void AddSnapshotRegions(IEnumerable<SnapshotRegion> snapshotRegions)
        {
            this.AddSnapshotRegions(snapshotRegions?.ToArray());
        }

        /// <summary>
        /// Adds memory regions to the regions contained in this snapshot. Will automatically merge and sort regions.
        /// </summary>
        /// <param name="snapshotRegions">The snapshot regions to add.</param>
        public void AddSnapshotRegions(params SnapshotRegion[] snapshotRegions)
        {
            this.MemoryRegions = new List<MemoryRegion>();

            snapshotRegions?.ForEach(snapshotRegion => this.MemoryRegions.Add(new MemoryRegion(snapshotRegion)));

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