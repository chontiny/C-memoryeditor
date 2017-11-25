namespace Squalr.Source.Snapshots
{
    using Squalr.Source.Scanners.ScanConstraints;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a region of memory in an external process.
    /// </summary>
    internal class SnapshotRegion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        /// <param name="readGroup">The read group of this snapshot region.</param>
        /// <param name="baseAddress">The base address of this snapshot region.</param>
        /// <param name="regionSize">The size of this snapshot region.</param>
        public SnapshotRegion(ReadGroup readGroup, UInt64 readGroupOffset, UInt64 regionSize)
        {
            this.ReadGroup = readGroup;
            this.ReadGroupOffset = readGroupOffset;
            this.RegionSize = regionSize;
        }

        public ReadGroup ReadGroup { get; set; }

        public UInt64 ReadGroupOffset { get; set; }

        public UInt64 RegionSize { get; set; }

        public IntPtr BaseAddress
        {
            get
            {
                return this.ReadGroup.BaseAddress.Add(this.ReadGroupOffset);
            }
        }

        public IntPtr EndAddress
        {
            get
            {
                return this.ReadGroup.BaseAddress.Add(this.ReadGroupOffset + this.RegionSize);
            }
        }

        /// <summary>
        /// Gets or sets the base index of this snapshot. In other words, the index of the first element of this region in the scan results.
        /// </summary>
        public UInt64 BaseElementIndex { get; set; }

        public UInt64 ElementCount
        {
            get
            {
                return this.RegionSize / unchecked((UInt32)this.ReadGroup.Alignment);
            }
        }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        public SnapshotElementComparer this[UInt32 index]
        {
            get
            {
                return new SnapshotElementComparer(parent: this, elementIndex: index);
            }
        }

        public IList<SnapshotRegion> CompareAll(
            ScanConstraint.ConstraintType compareActionConstraint,
            Object compareActionValue = null)
        {
            if (!this.ReadGroup.CanCompare(ScanConstraint.IsRelativeConstraint(compareActionConstraint)))
            {
                return null;
            }

            UInt32 vectorSize = EngineCore.GetInstance().Architecture.GetVectorSize();

            SnapshotElementVectorComparer vectorComparer = new SnapshotElementVectorComparer(
                region: this,
                vectorSize: vectorSize,
                compareActionConstraint: compareActionConstraint,
                compareActionValue: compareActionValue);

            while (vectorComparer.VectorReadIndex < this.RegionSize)
            {
                vectorComparer.Compare();
                vectorComparer.VectorReadIndex += vectorSize;
            }

            return vectorComparer.GatherCollectedRegions();
        }

        /// <summary>
        /// Gets the enumerator for an element reference within this snapshot region.
        /// </summary>
        /// <param name="pointerIncrementMode">The method for incrementing pointers.</param>
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        /// <returns>The enumerator for an element reference within this snapshot region.</returns>
        public IEnumerator<SnapshotElementVectorComparer> IterateElements(
            ScanConstraint.ConstraintType compareActionConstraint = ScanConstraint.ConstraintType.Changed,
            Object compareActionValue = null)
        {
            yield break;
        }
    }
    //// End class
}
//// End namespace