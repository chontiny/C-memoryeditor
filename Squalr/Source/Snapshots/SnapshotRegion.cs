namespace Squalr.Source.Snapshots
{
    using Results.ScanResults;
    using Scanners.ScanConstraints;
    using Squalr.Properties;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.OperatingSystems;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a region of memory in an external process.
    /// </summary>
    public class SnapshotRegion : NormalizedRegion, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        public SnapshotRegion() : this(IntPtr.Zero, 0UL)
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
        public SnapshotRegion(IntPtr baseAddress, UInt64 regionSize) : base(baseAddress, regionSize)
        {
            this.TimeSinceLastRead = DateTime.MinValue;
            this.Alignment = SettingsViewModel.GetInstance().Alignment;
            this.ElementType = ScanResultsViewModel.GetInstance().ActiveType;
        }

        /// <summary>
        /// Gets the number of bytes that this snapshot spans.
        /// </summary>
        /// <returns>The number of bytes that this snapshot spans.</returns>
        public UInt64 ByteCount
        {
            get
            {
                return this.CurrentValues == null ? 0UL : this.CurrentValues.LongLength.ToUInt64();
            }
        }

        /// <summary>
        /// Gets the number of elements contained by this snapshot. Equal to ByteCount / Alignment.
        /// </summary>
        /// <returns>The number of elements contained by this snapshot.</returns>
        public UInt64 ElementCount
        {
            get
            {
                return (this.CurrentValues == null ? 0L : this.CurrentValues.LongLength / this.Alignment).ToUInt64();
            }
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
        public SnapshotElementIterator this[Int32 index]
        {
            get
            {
                return new SnapshotElementIterator(parent: this, elementIndex: index, pointerIncrementMode: PointerIncrementMode.AllPointers);
            }
        }

        /// <summary>
        /// Sets all valid bits to the specified boolean value.
        /// </summary>
        /// <param name="isValid">Value indicating if valid bits will be marked as valid or invalid.</param>
        public void SetAllValidBits(Boolean isValid)
        {
            this.ValidBits = new BitArray(this.RegionSize.ToInt32(), isValid);
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
        public Byte[] ReadAllRegionMemory(Boolean keepValues, out Boolean readSuccess)
        {
            this.TimeSinceLastRead = DateTime.Now;

            readSuccess = false;
            Byte[] newCurrentValues = EngineCore.GetInstance().OperatingSystem.ReadBytes(this.BaseAddress, this.RegionSize.ToInt32(), out readSuccess);

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
            Int32 elementSize = this.GetElementSize();

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
                    validRegionSize += elementSize;
                }
                while (startIndex + validRegionSize < this.ValidBits.Length && this.ValidBits[startIndex + validRegionSize]);

                // Create new subregion from this valid region
                SnapshotRegion subRegion = new SnapshotRegion(this.BaseAddress + startIndex, validRegionSize.ToUInt64());

                // Ensure region size is worth keeping. This can happen if we grab a misaligned segment
                if (subRegion.RegionSize < Conversions.GetTypeSize(this.ElementType).ToUInt64())
                {
                    continue;
                }

                // Copy the current values and labels.
                subRegion.SetCurrentValues(this.CurrentValues.LargestSubArray(startIndex, subRegion.RegionSize.ToInt64()));
                subRegion.SetPreviousValues(this.PreviousValues.LargestSubArray(startIndex, subRegion.RegionSize.ToInt64()));
                subRegion.SetElementLabels(this.ElementLabels.LargestSubArray(startIndex, subRegion.RegionSize.ToInt64()));

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
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        /// <returns>The enumerator for an element reference within this snapshot region.</returns>
        public IEnumerator<SnapshotElementIterator> IterateElements(
            PointerIncrementMode pointerIncrementMode,
            ConstraintsEnum compareActionConstraint = ConstraintsEnum.Changed,
            Object compareActionValue = null)
        {
            UInt64 elementCount = this.ElementCount;
            SnapshotElementIterator snapshotElement = new SnapshotElementIterator(
                parent: this,
                pointerIncrementMode: pointerIncrementMode,
                compareActionConstraint: compareActionConstraint,
                compareActionValue: compareActionValue);

            for (UInt64 index = 0; index < elementCount; index++)
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
            switch (Type.GetTypeCode(this.ElementType))
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