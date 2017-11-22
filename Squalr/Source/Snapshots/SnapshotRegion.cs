namespace Squalr.Source.Snapshots
{
    using Squalr.Source.Scanners.ScanConstraints;
    using SqualrCore.Source.Engine;
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
        public SnapshotRegion(ReadGroup readGroup, Int32 readGroupOffset, Int32 regionSize)
        {
            this.ReadGroup = readGroup;
            this.ReadGroupOffset = readGroupOffset;
            this.RegionSize = regionSize;
        }

        public ReadGroup ReadGroup { get; set; }

        public Int32 ReadGroupOffset { get; set; }

        public Int32 RegionSize { get; set; }

        public Int32 ElementCount
        {
            get
            {
                return this.RegionSize / this.ReadGroup.Alignment;
            }
        }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        public SnapshotElementIterator this[Int32 index]
        {
            get
            {
                return new SnapshotElementIterator(parent: this, elementIndex: index);
            }
        }

        /// <summary>
        /// Gets the enumerator for an element reference within this snapshot region.
        /// </summary>
        /// <param name="pointerIncrementMode">The method for incrementing pointers.</param>
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        /// <returns>The enumerator for an element reference within this snapshot region.</returns>
        public IEnumerator<SnapshotRegionComparer> IterateElements(
            ScanConstraint.ConstraintType compareActionConstraint = ScanConstraint.ConstraintType.Changed,
            Object compareActionValue = null)
        {
            if (!this.ReadGroup.CanCompare(ScanConstraint.IsRelativeConstraint(compareActionConstraint)))
            {
                yield break;
            }

            SnapshotRegionComparer regionComparer = new SnapshotRegionComparer(
                parent: this,
                vectorSize: EngineCore.GetInstance().Architecture.GetVectorSize(),
                compareActionConstraint: compareActionConstraint,
                compareActionValue: compareActionValue);

            Int32 vectorSize = EngineCore.GetInstance().Architecture.GetVectorSize();

            while (regionComparer.VectorReadIndex < this.RegionSize)
            {
                yield return regionComparer;
                regionComparer.VectorReadIndex += vectorSize;
            }

            regionComparer.FinalizeSnapshots();
        }
    }
    //// End class
}
//// End namespace