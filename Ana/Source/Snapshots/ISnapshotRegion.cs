namespace Ana.Source.Snapshots
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Interface defining a region of memory in an external process.
    /// </summary>
    internal partial interface ISnapshotRegion : IEnumerable
    {
        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        ISnapshotElementRef this[Int32 index]
        {
            get;
        }

        /// <summary>
        /// Sets the memory alignment of this memory region.
        /// </summary>
        /// <param name="alignment">The memory alignment.</param>
        void SetAlignment(Int32 alignment);

        /// <summary>
        /// Sets all valid bits to the specified boolean value.
        /// </summary>
        /// <param name="isValid">Value indicating if valid bits will be marked as valid or invalid.</param>
        void SetAllValidBits(Boolean isValid);

        /// <summary>
        /// Expands a region by the element type size in both directions unconditionally.
        /// </summary>
        /// <param name="expandSize">The size by which to expand this region.</param>
        void Expand(Int32 expandSize);

        /// <summary>
        /// Sets the current values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        void SetCurrentValues(Byte[] newValues);

        /// <summary>
        /// Sets the previous values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        void SetPreviousValues(Byte[] newValues);

        /// <summary>
        /// Gets the time since values were read for this region.
        /// </summary>
        /// <returns>The time since values were read for this region.</returns>
        DateTime GetTimeSinceLastRead();

        /// <summary>
        /// Gets the valid bit array associated with this region.
        /// </summary>
        /// <returns>The valid bit array associated with this region.</returns>
        BitArray GetValidBits();

        /// <summary>
        /// Determines if a relative comparison can be done for this region, ie current and previous values are loaded.
        /// </summary>
        /// <returns>True if a relative comparison can be done for this region.</returns>
        Boolean CanCompare();

        /// <summary>
        /// Reads all memory for this snapshot region.
        /// </summary>
        /// <param name="readSuccess">Whether or not the read was successful.</param>
        /// <param name="keepValues">Whether or not to keep the values returned as current values.</param>
        /// <returns>The bytes read from memory.</returns>
        Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true);

        /// <summary>
        /// Determines if an address is contained in this snapshot.
        /// </summary>
        /// <param name="address">The address for which to search.</param>
        /// <returns>True if the address is contained.</returns>
        Boolean ContainsAddress(IntPtr address);

        /// <summary>
        /// Gets the current values as raw bytes of this snapshot.
        /// </summary>
        /// <returns>The current values as raw bytes of this snapshot.</returns>
        Byte[] GetCurrentValues();

        /// <summary>
        /// Gets the previous values as raw bytes of this snapshot.
        /// </summary>
        /// <returns>The previous values as raw bytes of this snapshot.</returns>
        Byte[] GetPreviousValues();

        /// <summary>
        /// Gets the number of bytes that this snapshot spans.
        /// </summary>
        /// <returns>The number of bytes that this snapshot spans.</returns>
        Int64 GetByteCount();

        /// <summary>
        /// Gets the number of elements contained by this snapshot. Equal to ByteCount / Alignment.
        /// </summary>
        /// <returns>The number of elements contained by this snapshot.</returns>
        Int32 GetElementCount();

        /// <summary>
        /// Gets the base address of this snapshot region.
        /// </summary>
        /// <returns>The base address of this snapshot region.</returns>
        IntPtr GetBaseAddress();

        /// <summary>
        /// Gets the end address of this snapshot region.
        /// </summary>
        /// <returns>The end address of this snapshot region.</returns>
        IntPtr GetEndAddress();
    }
    //// End interface

    /// <summary>
    /// Interface defining a region of memory in an external process.
    /// </summary>
    /// <typeparam name="DataType">The data type of this snapshot region.</typeparam>
    /// <typeparam name="LabelType">The type corresponding to the labels of this snapshot region.</typeparam>
    internal partial interface ISnapshotRegion<DataType, LabelType> : ISnapshotRegion
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {
        /// <summary>
        /// Sets the element labels for this snapshot region.
        /// </summary>
        /// <param name="newLabels">The new labels to be assigned.</param>
        void SetElementLabels(params LabelType[] newLabels);

        /// <summary>
        /// Gets the colletion of element labels for this snapshot region.
        /// </summary>
        /// <returns>The colletion of element labels for this snapshot region.</returns>
        LabelType[] GetElementLabels();

        /// <summary>
        /// Gets the regions in this snapshot with a valid bit set.
        /// </summary>
        /// <returns>The regions in this snapshot with a valid bit set.</returns>
        IEnumerable<ISnapshotRegion<DataType, LabelType>> GetValidRegions();
    }
    //// End interface
}
//// End namespace