using Ana.Source.Engine.OperatingSystems;
using Ana.Source.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ana.Source.Snapshots
{
    internal interface ISnapshotRegion<DataType, LabelType>
        where DataType : class, IComparable<DataType>
        where LabelType : class, IComparable<LabelType>
    {
        void SetAlignment(Int32 alignment);

        void SetAllValidBits(Boolean isValid);

        void Relax(Int32 relaxSize);

        void Expand(Int32 expandSize);

        void SetCurrentValues(Byte[] newValues);

        void SetPreviousValues(Byte[] newValues);

        void SetElementLabels(params LabelType[] newLabels);

        Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true);

        Boolean ContainsAddress(IntPtr address);

        Byte[] GetPreviousValues();

        Byte[] GetCurrentValues();

        LabelType[] GetElementLabels();

        IEnumerable<ISnapshotRegion<DataType, LabelType>> GetValidRegions();

        UInt64 GetMemorySize();
    }

    internal class NewSnapshotRegion<DataType, LabelType> : NormalizedRegion, ISnapshotRegion<DataType, LabelType>, IEnumerable
        where DataType : class, IComparable<DataType>
        where LabelType : class, IComparable<LabelType>
    {
        /// <summary>
        /// Gets or sets the most recently read values
        /// </summary>
        private unsafe Byte[] CurrentValues { get; set; }

        /// <summary>
        /// Gets or sets the previously read values
        /// </summary>
        private unsafe Byte[] PreviousValues { get; set; }

        /// <summary>
        /// Gets or sets the previously read values
        /// </summary>
        private unsafe LabelType[] ElementLabels { get; set; }

        /// <summary>
        /// Gets or sets the valid bits for use in filtering scans
        /// </summary>
        private BitArray Valid { get; set; }

        /// <summary>
        /// Gets or sets the memory alignment, typically aligned with external process pointer size
        /// </summary>
        private Int32 Alignment { get; set; }

        /// <summary>
        /// Gets or sets the reference to the snapshot element being iterated over
        /// </summary>
        private ISnapshotElementRef<DataType, LabelType> SnapshotElementRef { get; set; }

        public NewSnapshotRegion() : this(IntPtr.Zero, 0)
        {
        }

        public NewSnapshotRegion(IntPtr baseAddress, Int32 regionSize) : base(baseAddress, regionSize)
        {
        }

        public void SetAlignment(Int32 alignment)
        {
            this.Alignment = alignment;
        }

        public void SetAllValidBits(Boolean isValid)
        {
            this.Valid = new BitArray(this.RegionSize, isValid);
        }

        public void Relax(Int32 relaxSize)
        {
        }

        public void Expand(Int32 expandSize)
        {
            this.BaseAddress = this.BaseAddress.Subtract(expandSize);
            this.RegionSize += expandSize;
        }

        public void SetCurrentValues(Byte[] newValues)
        {
            this.CurrentValues = newValues;
        }

        public void SetPreviousValues(Byte[] newValues)
        {
        }

        public void SetElementLabels(params LabelType[] newLabels)
        {
        }

        public Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true)
        {
            readSuccess = false;
            return null;
        }

        public Boolean ContainsAddress(IntPtr address)
        {
            return false;
        }

        public Byte[] GetPreviousValues()
        {
            return this.PreviousValues;
        }

        public Byte[] GetCurrentValues()
        {
            return this.CurrentValues;
        }

        public LabelType[] GetElementLabels()
        {
            return this.ElementLabels;
        }

        public IEnumerable<ISnapshotRegion<DataType, LabelType>> GetValidRegions()
        {
            return null;
        }

        public UInt64 GetMemorySize()
        {
            return unchecked((UInt64)this.CurrentValues.LongLength);
        }

        public IEnumerator GetEnumerator()
        {
            if (this.RegionSize <= 0 || this.Alignment <= 0)
            {
                yield break;
            }

            // Prevent the GC from moving buffers around
            GCHandle currentValuesHandle = GCHandle.Alloc(this.CurrentValues, GCHandleType.Pinned);
            GCHandle previousValuesHandle = GCHandle.Alloc(this.PreviousValues, GCHandleType.Pinned);

            this.SnapshotElementRef = new SnapshotElementRef<DataType, LabelType>(this);

            this.SnapshotElementRef.InitializePointers();

            // Return the first element. This allows us to call IncrementPointers each loop unconditionally based on alignment later.
            yield return this.SnapshotElementRef;

            if (this.Alignment == 1)
            {
                // Utilizes ++ operator. This is fast operation wise, but slower because we check every address
                for (Int32 index = 1; index < this.RegionSize; index++)
                {
                    this.SnapshotElementRef.IncrementPointers();
                    yield return this.SnapshotElementRef;
                }
            }
            else
            {
                // Utilizes += operators. This is faster because we access far less addresses
                for (Int32 index = this.Alignment; index < this.RegionSize; index += this.Alignment)
                {
                    this.SnapshotElementRef.AddPointers(this.Alignment);
                    yield return this.SnapshotElementRef;
                }
            }

            // Let the GC do what it wants now
            currentValuesHandle.Free();
            previousValuesHandle.Free();
        }
    }
    //// End class
}
//// End namespace