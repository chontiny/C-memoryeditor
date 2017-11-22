namespace Squalr.Source.Snapshots
{
    using SqualrCore.Source.Engine.Types;
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
            this.ReadGroups = new List<ReadGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="memoryRegions">The regions with which to initialize this snapshot.</param>
        public Snapshot(String snapshotName, IEnumerable<ReadGroup> memoryRegions)
        {
            this.SnapshotName = snapshotName == null ? String.Empty : snapshotName;
            this.ReadGroups = memoryRegions?.ToList();
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
        public Snapshot(String snapshotName, IEnumerable<SnapshotRegion> snapshotRegions) : this(snapshotName)
        {
            this.AddSnapshotRegions(snapshotRegions);
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
                return this.SnapshotRegions?.Sum(x => x.RegionSize.ToUInt64()) ?? 0UL;
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
                return this.SnapshotRegions?.Sum(region => region.ElementCount.ToUInt64()) ?? 0UL;
            }
        }

        public DataType LabelDataType
        {
            set
            {
                this.ReadGroups.ForEach(readGroup => readGroup.LabelDataType = value);
            }
        }

        public Int32 Alignment
        {
            set
            {
                this.ReadGroups.ForEach(readGroup => readGroup.Alignment = value);
            }
        }

        public DataType ElementDataType
        {
            set
            {
                this.ReadGroups.ForEach(readGroup => readGroup.ElementDataType = value);
            }
        }

        /// <summary>
        /// Gets the time since the last update was performed on this snapshot.
        /// </summary>
        public DateTime TimeSinceLastUpdate { get; private set; }

        public IList<ReadGroup> ReadGroups;

        /// <summary>
        /// Gets the read groups in this snapshot, ordered descending by their region size. This is much more performant for multi-threaded access.
        /// </summary>
        public IEnumerable<ReadGroup> OptimizedReadGroups
        {
            get
            {
                return this.ReadGroups.OrderByDescending(readGroup => readGroup.RegionSize);
            }
        }

        public IEnumerable<SnapshotRegion> SnapshotRegions
        {
            get
            {
                return this.ReadGroups?.SelectMany(readGroup => readGroup.SnapshotRegions);
            }
        }

        /// <summary>
        /// Gets the snapshot regions in this snapshot, ordered descending by their region size. This is much more performant for multi-threaded access.
        /// This is very similar to the greedy interval scheduling algorithm, and can result in significant scan speed gains.
        /// </summary>
        public IEnumerable<SnapshotRegion> OptimizedSnapshotRegions
        {
            get
            {
                return this.ReadGroups?.SelectMany(readGroup => readGroup.SnapshotRegions).OrderByDescending(region => region.RegionSize);
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
                foreach (SnapshotRegion region in this.SnapshotRegions)
                {
                    UInt64 elementCount = region.ElementCount.ToUInt64();

                    if (index >= elementCount)
                    {
                        index -= elementCount;
                    }
                    else
                    {
                        return region[(Int32)index * region.ReadGroup.Alignment];
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
            return new Snapshot(newSnapshotName, this.ReadGroups);
        }

        /// <summary>
        /// Reads all memory for every region contained in this snapshot. TODO: This is not parallel, nor does it track progress.
        /// </summary>
        public void ReadAllMemory()
        {
            this.TimeSinceLastUpdate = DateTime.Now;
            // this.Intersect(SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotManagerViewModel.SnapshotRetrievalMode.FromSettings));
            this.ReadGroups?.ForEach(x => x.ReadAllMemory());
        }

        /// <summary>
        /// Sets the label of every element in this snapshot to the same value.
        /// </summary>
        /// <typeparam name="LabelType">The data type of the label.</typeparam>
        /// <param name="label">The new snapshot label value.</param>
        public void SetElementLabels<LabelType>(LabelType label) where LabelType : struct, IComparable<LabelType>
        {
            this.SnapshotRegions?.ForEach(x => x.ReadGroup.SetElementLabels(Enumerable.Repeat(label, x.RegionSize).Cast<Object>().ToArray()));
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
            this.ReadGroups = new List<ReadGroup>();

            IEnumerable<IGrouping<ReadGroup, SnapshotRegion>> snapshotsByReadGroup = snapshotRegions.GroupBy(region => region.ReadGroup);

            foreach (IGrouping<ReadGroup, SnapshotRegion> group in snapshotsByReadGroup)
            {
                group.Key.SnapshotRegions.Clear();

                foreach (SnapshotRegion region in group)
                {
                    group.Key.SnapshotRegions.Add(region);
                }

                group.Key.SnapshotRegions = group.Key.SnapshotRegions.OrderBy(region => region.ReadGroupOffset).ToList();

                this.ReadGroups.Add(group.Key);
            }

            this.ReadGroups = this.ReadGroups.OrderBy(group => group.BaseAddress.ToUInt64()).ToList();
        }
    }
    //// End class
}
//// End namespace