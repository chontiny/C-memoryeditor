namespace Ana.Source.Snapshots
{
    using Engine.OperatingSystems;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Interface defining a snapshot of memory in an external process.
    /// </summary>
    internal interface ISnapshot : IEnumerable
    {
        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index. Notes: This does NOT index into a region. 
        /// An individual region is only an Int32, but there may be many of these, so the indexer requires Int64.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        ISnapshotElementRef this[Int64 index]
        {
            get;
        }

        /// <summary>
        /// Sets the element type of the snapshot.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        void SetElementType(Type elementType);

        /// <summary>
        /// Sets the label type of the snapshot.
        /// </summary>
        /// <param name="labelType">The label type.</param>
        void SetLabelType(Type labelType);

        /// <summary>
        /// Sets the memory alignment of this snapshot and all of the regions it contains.
        /// </summary>
        /// <param name="alignment">The memory alignment.</param>
        void SetAlignment(Int32 alignment);

        /// <summary>
        /// Sets all valid bits to the specified boolean value for each memory region contained.
        /// </summary>
        /// <param name="isValid">Value indicating if valid bits will be marked as valid or invalid.</param>
        void SetAllValidBits(Boolean isValid);

        /// <summary>
        /// Discards all sections of memory marked with invalid bits.
        /// </summary>
        void DiscardInvalidRegions();

        /// <summary>
        /// Unconditionally expands all regions in this snapshot by the specified size.
        /// </summary>
        /// <param name="expandSize">The size by which to expand the snapshot regions.</param>
        void ExpandAllRegions(Int32 expandSize);

        /// <summary>
        /// Reads all memory for every region contained in this snapshot.
        /// </summary>
        void ReadAllMemory();

        /// <summary>
        /// Masks the given memory regions against the memory regions of a given snapshot, keeping the common elements of the two in O(n).
        /// </summary>
        /// <param name="groundTruth">The snapshot containing the regions to mask the target regions against.</param>
        void MaskRegions(ISnapshot groundTruth);

        /// <summary>
        /// Masks the given memory regions against the given memory regions, keeping the common elements of the two in O(n).
        /// </summary>
        /// <param name="groundTruth">The snapshot regions to mask the target regions against.</param>
        void MaskRegions(IEnumerable<NormalizedRegion> groundTruth);

        Type GetElementType();

        Type GetLabelType();

        /// <summary>
        /// Gets the time since the last update was performed on this snapshot.
        /// </summary>
        /// <returns>The time since the last update.</returns>
        DateTime GetTimeSinceLastUpdate();

        /// <summary>
        /// Gets all snapshot regions contained in this snapshot
        /// </summary>
        /// <returns>The snapshot regions contained in this snapshot</returns>
        IEnumerable<ISnapshotRegion> GetSnapshotRegions();

        /// <summary>
        /// Creates a shallow clone of this snapshot.
        /// </summary>
        /// <param name="newSnapshotName">The snapshot generation method name.</param>
        /// <returns>The shallow cloned snapshot.</returns>
        ISnapshot Clone(String newSnapshotName = null);

        /// <summary>
        /// Determines if an address is contained in this snapshot.
        /// </summary>
        /// <param name="address">The address for which we are searching.</param>
        /// <returns>True if the address is contained.</returns>
        Boolean ContainsAddress(IntPtr address);

        /// <summary>
        /// Gets the number of regions contained in this snapshot.
        /// </summary>
        /// <returns>The number of regions contained in this snapshot.</returns>
        Int32 GetRegionCount();

        /// <summary>
        /// Gets the number of individual elements contained in this snapshot.
        /// </summary>
        /// <returns>The number of individual elements contained in this snapshot.</returns>
        Int64 GetElementCount();

        /// <summary>
        /// Gets the number of bytes contained in this snapshot.
        /// </summary>
        /// <returns>The number of bytes contained in this snapshot.</returns>
        Int64 GetByteCount();

        /// <summary>
        /// Adds snapshot regions to the regions contained in this snapshot. Will automatically merge and sort regions.
        /// </summary>
        /// <param name="snapshotRegions">The snapshot regions to add.</param>
        void AddSnapshotRegions(IEnumerable<ISnapshotRegion> snapshotRegions);

        /// <summary>
        /// Sets the label of every element in this snapshot to the same value.
        /// </summary>
        /// <param name="label">The new snapshot label value.</param>
        void SetElementLabels<LabelType>(LabelType label) where LabelType : struct, IComparable<LabelType>;

        /// <summary>
        /// Creates a shallow clone of this snapshot as a new data type.
        /// </summary>
        /// <typeparam name="NewDataType">The new data type.</typeparam>
        /// <param name="newSnapshotName">The snapshot generation method name.</param>
        /// <returns>The shallow cloned snapshot.</returns>
        ISnapshot CloneAs<NewDataType>(String newSnapshotName = null) where NewDataType : struct, IComparable<NewDataType>;
    }
    //// End interface
}
//// End namespace