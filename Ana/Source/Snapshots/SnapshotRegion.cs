namespace Ana.Source.Snapshots
{
    using Engine;
    using Engine.OperatingSystems;
    using Output;
    using Results.ScanResults;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UserSettings;
    using Utils;
    using Utils.Extensions;

    /// <summary>
    /// Defines a region of memory in an external process.
    /// </summary>
    internal class SnapshotRegion : NormalizedRegion, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        public SnapshotRegion() : this(IntPtr.Zero, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        /// <param name="normalizedRegion">The region on which to base this snapshot region.</param>
        public SnapshotRegion(NormalizedRegion normalizedRegion) :
            this(normalizedRegion == null ? IntPtr.Zero : normalizedRegion.BaseAddress, normalizedRegion == null ? 0 : normalizedRegion.RegionSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this snapshot region.</param>
        /// <param name="regionSize">The size of this snapshot region.</param>
        public SnapshotRegion(IntPtr baseAddress, Int32 regionSize) : base(baseAddress, regionSize)
        {
            this.TimeSinceLastRead = DateTime.MinValue;
            this.Alignment = SettingsViewModel.GetInstance().Alignment;
            this.ElementType = ScanResultsViewModel.GetInstance().ActiveType;
        }

        /// <summary>
        /// Gets or sets the data type of the elements of this region.
        /// </summary>
        public Type ElementType { get; set; }

        /// <summary>
        /// Gets or sets the data type of the labels of this region.
        /// </summary>
        public Type LabelType { get; set; }

        /// <summary>
        /// Gets the most recently read values.
        /// </summary>
        public unsafe Byte[] CurrentValues { get; private set; }

        /// <summary>
        /// Gets the previously read values.
        /// </summary>
        public unsafe Byte[] PreviousValues { get; private set; }

        /// <summary>
        /// Gets the element labels.
        /// </summary>
        public unsafe Object[] ElementLabels { get; private set; }

        /// <summary>
        /// Gets or sets the valid bits for use in filtering scans.
        /// </summary>
        private BitArray ValidBits { get; set; }

        /// <summary>
        /// Gets or sets the time since the last read was performed on this region.
        /// </summary>
        private DateTime TimeSinceLastRead { get; set; }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>Returns the snapshot element at the specified index.</returns>
        public SnapshotElementRef this[Int32 index]
        {
            get
            {
                return new SnapshotElementRef(this, PointerIncrementMode.AllPointers, index);
            }
        }

        /// <summary>
        /// Sets all valid bits to the specified boolean value.
        /// </summary>
        /// <param name="isValid">Value indicating if valid bits will be marked as valid or invalid.</param>
        public void SetAllValidBits(Boolean isValid)
        {
            this.ValidBits = new BitArray(this.RegionSize, isValid);
        }

        /// <summary>
        /// Sets the current values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        public void SetCurrentValues(Byte[] newValues)
        {
            this.CurrentValues = newValues;
        }

        /// <summary>
        /// Sets the previous values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        public void SetPreviousValues(Byte[] newValues)
        {
            this.PreviousValues = newValues;
        }

        /// <summary>
        /// Gets the number of bytes that this snapshot spans.
        /// </summary>
        /// <returns>The number of bytes that this snapshot spans.</returns>
        public Int64 GetByteCount()
        {
            return this.CurrentValues == null ? 0L : this.CurrentValues.LongLength;
        }

        /// <summary>
        /// Gets the number of elements contained by this snapshot. Equal to ByteCount / Alignment.
        /// </summary>
        /// <returns>The number of elements contained by this snapshot.</returns>
        public Int32 GetElementCount()
        {
            return unchecked((Int32)(this.CurrentValues == null ? 0L : this.CurrentValues.LongLength / this.Alignment));
        }

        /// <summary>
        /// Gets the time since values were read for this region.
        /// </summary>
        /// <returns>The time since values were read for this region.</returns>
        public DateTime GetTimeSinceLastRead()
        {
            return this.TimeSinceLastRead;
        }

        /// <summary>
        /// Gets the valid bit array associated with this region.
        /// </summary>
        /// <returns>The valid bit array associated with this region.</returns>
        public BitArray GetValidBits()
        {
            return this.ValidBits;
        }

        /// <summary>
        /// Sets the element labels for this snapshot region.
        /// </summary>
        /// <param name="newLabels">The new labels to be assigned.</param>
        public void SetElementLabels(Object[] newLabels)
        {
            this.ElementLabels = newLabels;
        }

        /// <summary>
        /// Determines if a relative comparison can be done for this region, ie current and previous values are loaded.
        /// </summary>
        /// <returns>True if a relative comparison can be done for this region.</returns>
        public Boolean CanCompare()
        {
            if (this.PreviousValues == null || this.CurrentValues == null || this.PreviousValues.Length != this.CurrentValues.Length)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads all memory for this snapshot region.
        /// </summary>
        /// <param name="readSuccess">Whether or not the read was successful.</param>
        /// <param name="keepValues">Whether or not to keep the values returned as current values.</param>
        /// <returns>The bytes read from memory.</returns>
        public Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true)
        {
            this.TimeSinceLastRead = DateTime.Now;

            readSuccess = false;
            Byte[] newCurrentValues = EngineCore.GetInstance().OperatingSystemAdapter.ReadBytes(this.BaseAddress, this.RegionSize, out readSuccess);

            if (!readSuccess)
            {
                return null;
            }

            if (keepValues)
            {
                this.SetPreviousValues(this.CurrentValues);
                this.SetCurrentValues(newCurrentValues);
            }

            return newCurrentValues;
        }

        /// <summary>
        /// Gets the regions in this snapshot with a valid bit set.
        /// </summary>
        /// <returns>The regions in this snapshot with a valid bit set.</returns>
        public IEnumerable<SnapshotRegion> GetValidRegions()
        {
            List<SnapshotRegion> validRegions = new List<SnapshotRegion>();

            if (this.ValidBits == null)
            {
                return validRegions;
            }

            for (Int32 startIndex = 0; startIndex < this.ValidBits.Length; startIndex += this.Alignment)
            {
                if (!this.ValidBits[startIndex])
                {
                    continue;
                }

                // Get the length of this valid segment
                Int32 validRegionSize = 0;
                do
                {
                    // We only care if the aligned elements are valid
                    validRegionSize += this.Alignment;
                }
                while (startIndex + validRegionSize < this.ValidBits.Length && this.ValidBits[startIndex + validRegionSize]);

                // Create new subregion from this valid region
                SnapshotRegion subRegion = new SnapshotRegion(this.BaseAddress + startIndex, validRegionSize);

                // Ensure region size is worth keeping. This can happen if we grab a misaligned segment
                if (subRegion.RegionSize < Conversions.GetTypeSize(this.ElementType))
                {
                    continue;
                }

                // Copy the current values and labels.
                subRegion.SetCurrentValues(this.CurrentValues.LargestSubArray(startIndex, subRegion.RegionSize));
                subRegion.SetPreviousValues(this.PreviousValues.LargestSubArray(startIndex, subRegion.RegionSize));
                subRegion.SetElementLabels(this.ElementLabels.LargestSubArray(startIndex, subRegion.RegionSize));

                validRegions.Add(subRegion);
                startIndex += validRegionSize;
            }

            this.ValidBits = null;
            return validRegions;
        }

        /// <summary>
        /// Gets the enumerator for an element reference within this snapshot region.
        /// </summary>
        /// <param name="pointerIncrementMode">The method for incrementing pointers.</param>
        /// <returns>The enumerator for an element reference within this snapshot region.</returns>
        public IEnumerator<SnapshotElementRef> IterateElements(PointerIncrementMode pointerIncrementMode)
        {
            Int32 elementCount = this.GetElementCount();
            SnapshotElementRef snapshotElement = new SnapshotElementRef(this, pointerIncrementMode);

            for (Int32 index = 0; index < elementCount; index++)
            {
                yield return snapshotElement;
                snapshotElement.IncrementPointers();
            }
        }

        /// <summary>
        /// Gets the enumerator for an element reference within this snapshot region.
        /// </summary>
        /// <returns>The enumerator for an element reference within this snapshot region.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.IterateElements(PointerIncrementMode.AllPointers);
        }

        /// <summary>
        /// Gets the element size of the current data type.
        /// </summary>
        /// <returns>The element size of the current data type.</returns>
        private Int32 GetElementSize()
        {
            // Switch on type code. Could also do Marshal.SizeOf(DataType), but it is slower
            switch (Type.GetTypeCode(this.GetElementType()))
            {
                case TypeCode.Byte:
                    return sizeof(Byte);
                case TypeCode.SByte:
                    return sizeof(SByte);
                case TypeCode.Int16:
                    return sizeof(Int16);
                case TypeCode.Int32:
                    return sizeof(Int32);
                case TypeCode.Int64:
                    return sizeof(Int64);
                case TypeCode.UInt16:
                    return sizeof(UInt16);
                case TypeCode.UInt32:
                    return sizeof(UInt32);
                case TypeCode.UInt64:
                    return sizeof(UInt64);
                case TypeCode.Single:
                    return sizeof(Single);
                case TypeCode.Double:
                    return sizeof(Double);
                default:
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Invalid element type");
                    return 0;
            }
        }
    }
    //// End class
}
//// End namespace