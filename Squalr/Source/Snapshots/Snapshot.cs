namespace Squalr.Source.Snapshots
{
    using Results.ScanResults;
    using Squalr.Properties;
    using SqualrCore.Source.Engine.OperatingSystems;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a snapshot of memory in an external process.
    /// </summary>
    internal class Snapshot : IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(String snapshotName = null)
        {
            this.TimeSinceLastUpdate = DateTime.Now;
            this.SnapshotName = snapshotName == null ? String.Empty : snapshotName;
            this.SnapshotRegions = new List<SnapshotRegion>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot" /> class.
        /// </summary>
        /// <param name="snapshotRegions">The regions with which to initialize this snapshot.</param>
        /// <param name="snapshotName">The snapshot generation method name.</param>
        public Snapshot(IEnumerable<SnapshotRegion> snapshotRegions, String snapshotName = null) : this(snapshotName)
        {
            this.AddSnapshotRegions(snapshotRegions);
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
        /// Gets the number of regions contained in this snapshot.
        /// </summary>
        /// <returns>The number of regions contained in this snapshot.</returns>
        public Int32 RegionCount
        {
            get
            {
                return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.Count;
            }
        }

        /// <summary>
        /// Gets the total number of bytes contained in this snapshot.
        /// </summary>
        public UInt64 ByteCount
        {
            get
            {
                return this.SnapshotRegions == null ? 0L : this.SnapshotRegions.AsEnumerable().Sum(x => x.ByteCount);
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
                return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.AsEnumerable().Sum(x => x.ElementCount);
            }
        }

        /// <summary>
        /// Gets or sets the snapshot regions contained in this snapshot
        /// </summary>
        private IList<SnapshotRegion> SnapshotRegions { get; set; }

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

                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Invalid snapshot index");
                return null;
            }
        }

        /// <summary>
        /// Sets the data type of the labels contained in this snapshot.
        /// </summary>
        /// <param name="labelType"></param>
        public void SetLabelType(Type labelType)
        {
            this.SnapshotRegions?.ForEach(x => x.LabelType = labelType);
        }

        /// <summary>
        /// Updates the alignment and type settings of all snapshot regions.
        /// </summary>
        public void PropagateSettings()
        {
            if (this.SnapshotRegions == null)
            {
                return;
            }

            Type activeType = ScanResultsViewModel.GetInstance().ActiveType;
            Int32 alignment = SettingsViewModel.GetInstance().Alignment;

            foreach (SnapshotRegion region in this)
            {
                region.ElementType = activeType;
                region.Alignment = alignment;
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
            this.SnapshotRegions?.ForEach(x => validRegions.AddRange(x.GetValidRegions()));
            this.SnapshotRegions = validRegions;
        }

        /// <summary>
        /// Unconditionally expands all regions in this snapshot by the specified size.
        /// </summary>
        /// <param name="expandSize">The size by which to expand the snapshot regions.</param>
        public void ExpandAllRegions(UInt64 expandSize)
        {
            this.SnapshotRegions?.ForEach(x => x.Expand(expandSize));
            this.Intersect(SnapshotManager.GetInstance().CreateSnapshotFromSettings());
        }

        /// <summary>
        /// Reads all memory for every region contained in this snapshot. TODO: This is not parallel, nor does it track progress.
        /// </summary>
        public void ReadAllMemory()
        {
            this.TimeSinceLastUpdate = DateTime.Now;
            this.Intersect(SnapshotManager.GetInstance().CreateSnapshotFromSettings());
            this.SnapshotRegions?.ForEach(x => x.ReadAllMemory(keepValues: true, readSuccess: out _));
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

            this.PropagateSettings();
            this.MergeAndSortRegions();
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
        public Boolean ContainsAddress(UInt64 address)
        {
            if (this.SnapshotRegions == null || this.SnapshotRegions.Count == 0)
            {
                return false;
            }

            return this.ContainsAddressHelper(address, this.SnapshotRegions.Count / 2, 0, this.SnapshotRegions.Count);
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
        private Boolean ContainsAddressHelper(UInt64 address, Int32 middle, Int32 min, Int32 max)
        {
            if (middle < 0 || middle == this.SnapshotRegions.Count || max < min)
            {
                return false;
            }

            if (address < this.SnapshotRegions.ElementAt(middle).BaseAddress.ToUInt64())
            {
                return this.ContainsAddressHelper(address, (min + middle - 1) / 2, min, middle - 1);
            }
            else if (address > this.SnapshotRegions.ElementAt(middle).EndAddress.ToUInt64())
            {
                return this.ContainsAddressHelper(address, (middle + 1 + max) / 2, middle + 1, max);
            }
            else
            {
                return true;
            }
        }

        private void Union(Snapshot otherSnapshot)
        {
            if (otherSnapshot == null)
            {
                return;
            }

            foreach (SnapshotRegion region in otherSnapshot)
            {
                this.SnapshotRegions.Add(region);
            }

            this.MergeAndSortRegions();
        }

        /// <summary>
        /// Combines the given memory regions with the given memory regions, only keeping the common elements of the two in O(nlogn + n).
        /// </summary>
        /// <param name="otherSnapshot">The snapshot regions to mask the target regions against.</param>
        private void Intersect(Snapshot otherSnapshot)
        {
            List<SnapshotRegion> resultRegions = new List<SnapshotRegion>();

            otherSnapshot.MergeAndSortRegions();
            IEnumerable<NormalizedRegion> otherRegions = otherSnapshot.GetSnapshotRegions();

            if (this.SnapshotRegions.IsNullOrEmpty() || otherRegions.IsNullOrEmpty())
            {
                this.SnapshotRegions = resultRegions;
                return;
            }

            this.MergeAndSortRegions();

            // Initialize stacks with regions and masking regions
            Queue<SnapshotRegion> snapshotRegionQueue = new Queue<SnapshotRegion>();
            Queue<NormalizedRegion> groundTruthQueue = new Queue<NormalizedRegion>();

            // Build candidate region queue from snapshot region array
            foreach (SnapshotRegion region in this.SnapshotRegions.OrderBy(x => x.BaseAddress.ToUInt64()))
            {
                snapshotRegionQueue.Enqueue(region);
            }

            // Build masking region queue from snapshot
            foreach (NormalizedRegion maskRegion in otherRegions.OrderBy(x => x.BaseAddress.ToUInt64()))
            {
                groundTruthQueue.Enqueue(maskRegion);
            }

            if (snapshotRegionQueue.Count <= 0 || groundTruthQueue.Count <= 0)
            {
                this.SnapshotRegions = resultRegions;
                return;
            }

            SnapshotRegion nextSnapshotRegion;
            NormalizedRegion groundTruthMask = groundTruthQueue.Dequeue();

            while (snapshotRegionQueue.Count > 0)
            {
                // Grab next region
                nextSnapshotRegion = snapshotRegionQueue.Dequeue();

                // Grab the next mask following the current region
                while (groundTruthMask.EndAddress.ToUInt64() < nextSnapshotRegion.BaseAddress.ToUInt64() && groundTruthQueue.Count > 0)
                {
                    groundTruthMask = groundTruthQueue.Dequeue();
                }

                // Check for mask completely removing this region
                if (groundTruthMask.EndAddress.ToUInt64() < nextSnapshotRegion.BaseAddress.ToUInt64() || groundTruthMask.BaseAddress.ToUInt64() > nextSnapshotRegion.EndAddress.ToUInt64())
                {
                    continue;
                }
                // Check for mask completely engulfing this region
                else if (groundTruthMask.BaseAddress.ToUInt64() <= nextSnapshotRegion.BaseAddress.ToUInt64() && groundTruthMask.EndAddress.ToUInt64() >= nextSnapshotRegion.EndAddress.ToUInt64())
                {
                    resultRegions.Add(nextSnapshotRegion);
                    continue;
                }
                // There are no edge cases, we must mask and copy the valid portion of this region
                else
                {
                    UInt64 baseAddress = Math.Max(groundTruthMask.BaseAddress.ToUInt64(), nextSnapshotRegion.BaseAddress.ToUInt64());
                    UInt64 endAddress = Math.Min(groundTruthMask.EndAddress.ToUInt64(), nextSnapshotRegion.EndAddress.ToUInt64());
                    Int64 baseOffset = unchecked((Int64)(baseAddress - nextSnapshotRegion.BaseAddress.ToUInt64()));

                    SnapshotRegion newRegion = new SnapshotRegion(nextSnapshotRegion as NormalizedRegion);
                    newRegion.BaseAddress = baseAddress.ToIntPtr();
                    newRegion.EndAddress = endAddress.ToIntPtr();
                    newRegion.SetCurrentValues(nextSnapshotRegion.CurrentValues.LargestSubArray(baseOffset, newRegion.RegionSize.ToInt64()));
                    newRegion.SetPreviousValues(nextSnapshotRegion.PreviousValues.LargestSubArray(baseOffset, newRegion.RegionSize.ToInt64()));
                    newRegion.SetElementLabels(nextSnapshotRegion.ElementLabels.LargestSubArray(baseOffset, newRegion.RegionSize.ToInt64()));
                    newRegion.ElementType = nextSnapshotRegion.ElementType;
                    newRegion.Alignment = nextSnapshotRegion.Alignment;
                    resultRegions.Add(newRegion);
                }
            }

            this.SnapshotRegions = resultRegions;
        }

        private void Except(Snapshot otherSnapshot)
        {
            throw new NotImplementedException();
        }

        private void Subtract(Snapshot otherSnapshot)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Merges regions in the current list of memory regions using a fast stack based algorithm O(nlogn + n).
        /// </summary>
        private void MergeAndSortRegions()
        {
            if (this.SnapshotRegions.IsNullOrEmpty())
            {
                return;
            }

            // First, sort by start address
            IEnumerable<SnapshotRegion> sortedRegions = this.SnapshotRegions.OrderBy(x => x.BaseAddress.ToUInt64());

            // Create and initialize the stack with the first region
            Stack<SnapshotRegion> combinedRegions = new Stack<SnapshotRegion>();
            combinedRegions.Push(sortedRegions.First());

            // Build the remaining regions
            foreach (SnapshotRegion region in sortedRegions.Skip(1))
            {
                SnapshotRegion top = combinedRegions.Peek();

                // If the regions do not overlap, the new region is the top region
                if (top.EndAddress.ToUInt64() < region.BaseAddress.ToUInt64())
                {
                    combinedRegions.Push(region);
                }
                // The regions are exactly adjacent; merge them
                else if (top.EndAddress.ToUInt64() == region.BaseAddress.ToUInt64())
                {
                    top.RegionSize = region.EndAddress.Subtract(top.BaseAddress).ToUInt64();

                    // Combine values and labels
                    top.SetElementLabels(top.ElementLabels?.Concat(region.ElementLabels));
                    top.SetCurrentValues(top.CurrentValues?.Concat(region.CurrentValues));
                    top.SetPreviousValues(top.PreviousValues?.Concat(region.PreviousValues));
                }
                // The regions overlap
                else if (top.EndAddress.ToUInt64() <= region.EndAddress.ToUInt64())
                {
                    top.RegionSize = region.EndAddress.Subtract(top.BaseAddress).ToUInt64();

                    Int32 overlapSize = unchecked((Int32)(region.EndAddress.ToUInt64() - top.EndAddress.ToUInt64()));

                    // Overlap has conflicting values, so we prioritize the top region and trim the current region
                    region.SetElementLabels(region.ElementLabels?.SubArray(overlapSize, region.RegionSize.ToInt32() - overlapSize));
                    region.SetCurrentValues(region.CurrentValues?.SubArray(overlapSize, region.RegionSize.ToInt32() - overlapSize));
                    region.SetPreviousValues(region.PreviousValues?.SubArray(overlapSize, region.RegionSize.ToInt32() - overlapSize));

                    // Combine values and labels
                    top.SetElementLabels(top.ElementLabels?.Concat(region.ElementLabels));
                    top.SetCurrentValues(top.CurrentValues?.Concat(region.CurrentValues));
                    top.SetPreviousValues(top.PreviousValues?.Concat(region.PreviousValues));
                }
            }

            this.SnapshotRegions = combinedRegions.ToList().OrderBy(x => x.BaseAddress.ToUInt64()).ToList();
        }
    }
    //// End class
}
//// End namespace
