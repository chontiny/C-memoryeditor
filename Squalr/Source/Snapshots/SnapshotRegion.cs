namespace Squalr.Source.Snapshots
{
    using Squalr.Properties;
    using Squalr.Source.Results;
    using Squalr.Source.Scanners.ScanConstraints;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Defines a region of memory in an external process.
    /// </summary>
    internal class SnapshotRegion : NormalizedRegion, IEnumerable<SnapshotRegionComparer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this snapshot region.</param>
        /// <param name="regionSize">The size of this snapshot region.</param>
        public SnapshotRegion(IntPtr baseAddress, UInt64 regionSize) : base(baseAddress, regionSize)
        {
            this.TimeSinceLastRead = DateTime.MinValue;
            this.Alignment = SettingsViewModel.GetInstance().Alignment;
            this.ElementDataType = ScanResultsViewModel.GetInstance().ActiveType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegion" /> class.
        /// </summary>
        /// <param name="readGroup">The read group of this snapshot region.</param>
        /// <param name="baseAddress">The base address of this snapshot region.</param>
        /// <param name="regionSize">The size of this snapshot region.</param>
        public SnapshotRegion(ReadGroup readGroup, IntPtr baseAddress, UInt64 regionSize) : this(baseAddress, regionSize)
        {
            this.ReadGroup = readGroup;
        }

        /// <summary>
        /// Gets or sets the data type of the elements of this region.
        /// </summary>
        public DataType ElementDataType { get; set; }

        /// <summary>
        /// Gets or sets the data type of the labels of this region.
        /// </summary>
        public DataType LabelDataType { get; set; }

        /// <summary>
        /// Gets the most recently read values.
        /// </summary>
        public unsafe Vector<Byte> CurrentValues
        {
            get
            {
                if (this.ReadGroup?.CurrentValues == null)
                {
                    return new Vector<Byte>();
                }

                return new Vector<Byte>(this.ReadGroup.CurrentValues, this.ReadGroupOffset);
            }
        }

        /// <summary>
        /// Gets the previously read values.
        /// </summary>
        public unsafe Vector<Byte> PreviousValues
        {
            get
            {
                if (this.ReadGroup?.PreviousValues == null)
                {
                    return new Vector<Byte>();
                }

                return new Vector<Byte>(this.ReadGroup.PreviousValues, this.ReadGroupOffset);
            }
        }

        /// <summary>
        /// Gets the element labels.
        /// </summary>
        public unsafe Object[] ElementLabels { get; private set; }

        /// <summary>
        /// Gets or sets the valid bits for use in filtering scans.
        /// </summary>
        public Byte[] ValidBits { get; set; }

        /// <summary>
        /// Gets or sets the time since the last read was performed on this region.
        /// </summary>
        private DateTime TimeSinceLastRead { get; set; }

        public ReadGroup ReadGroup { get; set; }

        public Int32 ReadGroupOffset
        {
            get
            {
                return (this.BaseAddress.Subtract(this.ReadGroup.BaseAddress)).ToInt32();
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
        /// Sets all valid bits to the specified boolean value.
        /// </summary>
        /// <param name="isValid">Value indicating if valid bits will be marked as valid or invalid.</param>
        public void SetAllValidBits(Boolean isValid)
        {
            this.ValidBits = new Byte[this.RegionSize.ToInt32() / this.Alignment];
        }

        /// <summary>
        /// Sets the current values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        public void SetCurrentValues(Byte[] newValues)
        {
            this.ReadGroup.SetCurrentValues(newValues);
        }

        /// <summary>
        /// Sets the previous values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        public void SetPreviousValues(Byte[] newValues)
        {
            this.ReadGroup.SetPreviousValues(newValues);
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
            if (this.PreviousValues == null || this.CurrentValues == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the regions in this snapshot with a valid bit set.
        /// </summary>
        /// <returns>The regions in this snapshot with a valid bit set.</returns>
        public IEnumerable<SnapshotRegion> GetValidRegions()
        {
            return null;
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
            UInt64 elementCount = this.ElementCount;
            SnapshotRegionComparer snapshotElement = new SnapshotRegionComparer(
                parent: this,
                compareActionConstraint: compareActionConstraint,
                compareActionValue: compareActionValue);
            Int32 vectorSize = EngineCore.GetInstance().Architecture.GetVectorSize();

            Int32 batchCount = (elementCount / (vectorSize / 4).ToUInt64()).ToInt32();

            for (Int32 index = 0; index < batchCount; index++)
            {
                yield return snapshotElement;
                snapshotElement.ElementIndex += vectorSize;
            }
        }

        /// <summary>
        /// Gets the enumerator for an element reference within this snapshot region.
        /// </summary>
        /// <returns>The enumerator for an element reference within this snapshot region.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.IterateElements();
        }

        IEnumerator<SnapshotRegionComparer> IEnumerable<SnapshotRegionComparer>.GetEnumerator()
        {
            return this.IterateElements();
        }
    }
    //// End class
}
//// End namespace