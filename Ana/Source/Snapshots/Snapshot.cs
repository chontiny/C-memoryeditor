namespace Ana.Source.Snapshots
{
    using Engine.OperatingSystems;
    using Results.ScanResults;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UserSettings;
    using Utils.Extensions;

    /// <summary>
    /// Defines a snapshot of memory in an external process.
    /// </summary>
    internal class Snapshot : IEnumerable
    {
        private Int32 alignment;

        private Type elementType;

        private Type labelType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName = null)
        {
            this.ElementType = ScanResultsViewModel.GetInstance().ActiveType;
            this.TimeSinceLastUpdate = DateTime.Now;
            this.SnapshotName = snapshotName == null ? String.Empty : snapshotName;
            this.SnapshotRegions = new List<SnapshotRegion>();
            this.Alignment = SettingsViewModel.GetInstance().Alignment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(IEnumerable<SnapshotRegion> snapshotRegions, String snapshotName = null) : this(snapshotName)
        {
            this.AddSnapshotRegions(snapshotRegions);
            this.ElementType = ScanResultsViewModel.GetInstance().ActiveType;
        }

        /// <summary>
        /// Gets the time since the last update was performed on this snapshot.
        /// </summary>
        public DateTime TimeSinceLastUpdate { get; private set; }

        /// <summary>
        /// Gets the name associated with the method by which this snapshot was generated.
        /// </summary>
        public String SnapshotName { get; private set; }

        /// <summary>
        /// Gets the total number of bytes contained in this snapshot.
        /// </summary>
        public Int64 ByteCount
        {
            get
            {
                return this.SnapshotRegions == null ? 0L : this.SnapshotRegions.AsEnumerable().Sum(x => x.GetByteCount());
            }
        }

        public Type ElementType
        {
            get
            {
                return this.elementType;
            }

            set
            {
                this.elementType = value;
                this.SnapshotRegions?.ForEach(x => x.ElementType = value);
            }
        }

        public Type LabelType
        {
            get
            {
                return this.labelType;
            }

            set
            {
                this.labelType = value;
                this.SnapshotRegions?.ForEach(x => x.LabelType = value);
            }
        }

        /// <summary>
        /// Gets or sets the memory alignment of the regions contained in the snapshot
        /// </summary>
        private Int32 Alignment
        {
            get
            {
                return this.alignment;
            }

            set
            {
                this.alignment = value <= 0 ? 1 : value;
                this.SnapshotRegions?.ForEach(x => x.Alignment = value);
            }
        }

        /// <summary>
        /// Gets or sets the snapshot regions contained in this snapshot
        /// </summary>
        private IList<SnapshotRegion> SnapshotRegions { get; set; }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index. Notes: This does NOT index into a region. 
        /// An individual region is only an Int32, but there may be many of these, so the indexer requires Int64.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        public SnapshotElementRef this[Int64 index]
        {
            get
            {
                foreach (SnapshotRegion region in this)
                {
                    Int64 elementCount = (Int64)region.GetElementCount();

                    if (index - elementCount >= 0)
                    {
                        index -= elementCount;
                    }
                    else
                    {
                        return region[(Int32)index * this.Alignment];
                    }
                }

                return null;
            }
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
            SnapshotRegions?.ForEach(x => validRegions.AddRange(x.GetValidRegions()));
            this.SnapshotRegions = validRegions;
        }

        /// <summary>
        /// Unconditionally expands all regions in this snapshot by the specified size.
        /// </summary>
        /// <param name="expandSize">The size by which to expand the snapshot regions.</param>
        public void ExpandAllRegions(Int32 expandSize)
        {
            this.SnapshotRegions?.ForEach(x => x.Expand(expandSize));
            this.MaskRegions(SnapshotManager.GetInstance().CollectSnapshotRegions(useSettings: false));
        }

        /// <summary>
        /// Reads all memory for every region contained in this snapshot.
        /// </summary>
        public void ReadAllMemory()
        {
            this.TimeSinceLastUpdate = DateTime.Now;
            Boolean readSuccess;
            this.SnapshotRegions?.ForEach(x => x.ReadAllRegionMemory(out readSuccess, keepValues: true));
        }

        /// <summary>
        /// Sets the label of every element in this snapshot to the same value.
        /// </summary>
        /// <param name="label">The new snapshot label value.</param>
        public void SetElementLabels<LabelType>(LabelType label) where LabelType : struct, IComparable<LabelType>
        {
            this.SnapshotRegions?.ForEach(x => x.SetElementLabels(label));
        }

        /// <summary>
        /// Adds snapshot regions to the regions contained in this snapshot. Will automatically merge and sort regions.
        /// </summary>
        /// <param name="snapshotRegions">The snapshot regions to add.</param>
        public void AddSnapshotRegions(IEnumerable<SnapshotRegion> snapshotRegions)
        {
            if (this.SnapshotRegions == null)
            {
                this.SnapshotRegions = snapshotRegions.ToList();
            }
            else
            {
                snapshotRegions?.ForEach(x => this.SnapshotRegions.Add(x));
            }

            this.MaskRegions(SnapshotManager.GetInstance().CollectSnapshotRegions(useSettings: false));
        }

        /// <summary>
        /// Masks the given memory regions against the memory regions of a given snapshot, keeping the common elements of the two in O(n).
        /// </summary>
        /// <param name="groundTruth">The snapshot containing the regions to mask the target regions against.</param>
        public void MaskRegions(Snapshot groundTruth)
        {
            this.MaskRegions(groundTruth.GetSnapshotRegions()?.Select(x => x as NormalizedRegion));
        }

        /// <summary>
        /// Masks the given memory regions against the given memory regions, keeping the common elements of the two in O(n).
        /// </summary>
        /// <param name="groundTruth">The snapshot regions to mask the target regions against.</param>
        public void MaskRegions(IEnumerable<NormalizedRegion> groundTruth)
        {
            List<SnapshotRegion> resultRegions = new List<SnapshotRegion>();

            if (this.SnapshotRegions == null || groundTruth == null || this.SnapshotRegions.Count <= 0 || groundTruth.Count() <= 0)
            {
                this.SnapshotRegions = resultRegions;
                return;
            }

            this.MergeAndSortRegions();

            // TODO: Confirm this masking shit even works
            return;

            // Initialize stacks with regions and masking regions
            Queue<SnapshotRegion> candidateRegions = new Queue<SnapshotRegion>();
            Queue<NormalizedRegion> maskingRegions = new Queue<NormalizedRegion>();

            // Build candidate region queue from target region array
            foreach (SnapshotRegion region in this.SnapshotRegions.OrderBy(x => x.BaseAddress.ToUInt64()))
            {
                candidateRegions.Enqueue(region);
            }

            // Build masking region queue from snapshot
            foreach (NormalizedRegion maskRegion in groundTruth.OrderBy(x => x.BaseAddress.ToUInt64()))
            {
                maskingRegions.Enqueue(maskRegion);
            }

            if (candidateRegions.Count <= 0 || maskingRegions.Count <= 0)
            {
                this.SnapshotRegions = resultRegions;
                return;
            }

            SnapshotRegion currentRegion;
            NormalizedRegion currentMask = maskingRegions.Dequeue();

            while (candidateRegions.Count > 0)
            {
                // Grab next region
                currentRegion = candidateRegions.Dequeue();

                // Grab the next mask following the current region
                while (currentMask.EndAddress.ToUInt64() < currentRegion.BaseAddress.ToUInt64() && maskingRegions.Count > 0)
                {
                    currentMask = maskingRegions.Dequeue();
                }

                // Check for mask completely removing this region
                if (currentMask.EndAddress.ToUInt64() < currentRegion.BaseAddress.ToUInt64() || currentMask.BaseAddress.ToUInt64() > currentRegion.EndAddress.ToUInt64())
                {
                    continue;
                }

                // Mask completely overlaps, just use the original region
                if (currentMask.BaseAddress == currentRegion.BaseAddress && currentMask.EndAddress == currentRegion.EndAddress)
                {
                    resultRegions.Add(currentRegion);
                    continue;
                }

                // Mask is within bounds; Grab the masked portion of this region
                Int32 baseOffset = currentMask.BaseAddress.ToUInt64() <= currentRegion.BaseAddress.ToUInt64() ? 0 : currentMask.BaseAddress.Subtract(currentRegion.BaseAddress).ToInt32();

                SnapshotRegion newRegion = new SnapshotRegion(currentRegion as NormalizedRegion);
                newRegion.BaseAddress = currentRegion.BaseAddress + baseOffset;
                newRegion.BaseAddress = Math.Min(currentMask.EndAddress.ToUInt64(), currentRegion.EndAddress.ToUInt64()).ToIntPtr();
                newRegion.SetCurrentValues(currentRegion.GetCurrentValues().LargestSubArray(baseOffset, newRegion.RegionSize));
                newRegion.SetPreviousValues(currentRegion.GetPreviousValues().LargestSubArray(baseOffset, newRegion.RegionSize));
                newRegion.SetElementLabels(currentRegion.GetElementLabels().LargestSubArray(baseOffset, newRegion.RegionSize));
                newRegion.ElementType = currentRegion.ElementType;
                newRegion.Alignment = currentRegion.Alignment;
                resultRegions.Add(newRegion);
            }

            this.SnapshotRegions = resultRegions;
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
        /// Gets all snapshot regions contained in this snapshot
        /// </summary>
        /// <returns>The snapshot regions contained in this snapshot</returns>
        public IEnumerable<SnapshotRegion> GetSnapshotRegions()
        {
            return this.SnapshotRegions;
        }

        /// <summary>
        /// Creates a shallow clone of this snapshot.
        /// </summary>
        /// <param name="newSnapshotName">The snapshot generation method name.</param>
        /// <returns>The shallow cloned snapshot.</returns>
        public Snapshot Clone(String newSnapshotName = null)
        {
            return new Snapshot(this.SnapshotRegions, newSnapshotName);
        }

        /// <summary>
        /// Determines if an address is contained in this snapshot.
        /// </summary>
        /// <param name="address">The address for which we are searching.</param>
        /// <returns>True if the address is contained.</returns>
        public Boolean ContainsAddress(IntPtr address)
        {
            if (this.SnapshotRegions == null || this.SnapshotRegions.Count() == 0)
            {
                return false;
            }

            return this.ContainsAddress(address, this.SnapshotRegions.Count() / 2, 0, this.SnapshotRegions.Count());
        }

        /// <summary>
        /// Gets the number of regions contained in this snapshot.
        /// </summary>
        /// <returns>The number of regions contained in this snapshot.</returns>
        public Int32 GetRegionCount()
        {
            return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.Count;
        }

        /// <summary>
        /// Gets the number of bytes contained in this snapshot.
        /// </summary>
        /// <returns>The number of bytes contained in this snapshot.</returns>
        public Int64 GetByteCount()
        {
            return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.AsEnumerable().Sum(x => x.GetByteCount());
        }

        /// <summary>
        /// Gets the number of individual elements contained in this snapshot.
        /// </summary>
        /// <returns>The number of individual elements contained in this snapshot.</returns>
        public Int64 GetElementCount()
        {
            return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.AsEnumerable().Sum(x => (Int64)x.GetElementCount());
        }

        /// <summary>
        /// Gets the snapshot region enumerator.
        /// </summary>
        /// <returns>The snapshot region enumerator.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.SnapshotRegions.GetEnumerator();
        }

        /// <summary>
        /// Helper function for searching for an address in this snapshot. Binary search that assumes this snapshot has sorted regions.
        /// </summary>
        /// <param name="address">The address for which we are searching.</param>
        /// <param name="middle">The middle region index.</param>
        /// <param name="min">The lower region index.</param>
        /// <param name="max">The upper region index.</param>
        /// <returns>True if the address was found.</returns>
        private Boolean ContainsAddress(IntPtr address, Int32 middle, Int32 min, Int32 max)
        {
            if (middle < 0 || middle == this.SnapshotRegions.Count() || max < min)
            {
                return false;
            }

            if (address.ToUInt64() < this.SnapshotRegions.ElementAt(middle).BaseAddress.ToUInt64())
            {
                return this.ContainsAddress(address, (min + middle - 1) / 2, min, middle - 1);
            }
            else if (address.ToUInt64() > this.SnapshotRegions.ElementAt(middle).EndAddress.ToUInt64())
            {
                return this.ContainsAddress(address, (middle + 1 + max) / 2, middle + 1, max);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Merges regions in the current list of memory regions using a fast stack based algorithm O(nlogn + n)
        /// </summary>
        private void MergeAndSortRegions()
        {
            if (this.SnapshotRegions == null || SnapshotRegions.Count <= 0)
            {
                return;
            }

            // First, sort by start address
            IList<SnapshotRegion> sortedRegions = this.SnapshotRegions.OrderBy(x => x.BaseAddress.ToUInt64()).ToList();

            // Create and initialize the stack with the first region
            Stack<SnapshotRegion> combinedRegions = new Stack<SnapshotRegion>();
            combinedRegions.Push(sortedRegions[0]);

            // Build the remaining regions
            for (Int32 index = combinedRegions.Count; index < sortedRegions.Count; index++)
            {
                SnapshotRegion top = combinedRegions.Peek();

                if (top.EndAddress.ToUInt64() < sortedRegions[index].BaseAddress.ToUInt64())
                {
                    // If the interval does not overlap, put it on the top of the stack
                    combinedRegions.Push(sortedRegions[index]);
                }
                else if (top.EndAddress.ToUInt64() == sortedRegions[index].BaseAddress.ToUInt64())
                {
                    // The regions are adjacent; merge them
                    top.RegionSize = sortedRegions[index].EndAddress.Subtract(top.BaseAddress).ToInt32();
                    top.SetElementLabels(top.GetElementLabels().Concat(sortedRegions[index].GetElementLabels()));
                    top.SetCurrentValues(top.GetCurrentValues()?.Concat(sortedRegions[index].GetCurrentValues()));
                    top.SetPreviousValues(top.GetPreviousValues()?.Concat(sortedRegions[index].GetPreviousValues()));
                }
                else if (top.EndAddress.ToUInt64() <= sortedRegions[index].EndAddress.ToUInt64())
                {
                    // The regions overlap
                    top.RegionSize = sortedRegions[index].EndAddress.Subtract(top.BaseAddress).ToInt32();

                    // TOOD: Previous/current value merging strategy
                }
            }

            this.SnapshotRegions = combinedRegions.ToList().OrderBy(x => x.BaseAddress.ToUInt64()).ToList();
        }
    }
    //// End class
}
//// End namespace
