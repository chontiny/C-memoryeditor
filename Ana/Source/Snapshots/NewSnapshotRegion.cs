using Ana.Source.Engine;
using Ana.Source.Engine.OperatingSystems;
using Ana.Source.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ana.Source.Snapshots
{
    internal interface ISnapshotRegion : IEnumerable
    {
        ISnapshotElementRef this[Int32 index]
        {
            get;
        }

        void SetAlignment(Int32 alignment);

        void SetAllValidBits(Boolean isValid);

        void Relax(Int32 relaxSize);

        void Expand(Int32 expandSize);

        void SetCurrentValues(Byte[] newValues);

        void SetPreviousValues(Byte[] newValues);

        DateTime GetTimeSinceLastRead();

        BitArray GetValidBits();

        Boolean CanCompare();

        Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true);

        Boolean ContainsAddress(IntPtr address);

        Byte[] GetPreviousValues();

        Byte[] GetCurrentValues();

        Int64 GetByteCount();

        Int32 GetElementCount();

        IntPtr GetBaseAddress();
    }

    internal interface ISnapshotRegion<DataType, LabelType> : ISnapshotRegion
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {
        void SetElementLabels(params LabelType[] newLabels);

        LabelType[] GetElementLabels();

        IEnumerable<ISnapshotRegion<DataType, LabelType>> GetValidRegions();
    }

    internal class NewSnapshotRegion<DataType, LabelType> : NormalizedRegion, ISnapshotRegion<DataType, LabelType>
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
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
        private BitArray ValidBits { get; set; }

        /// <summary>
        /// Gets or sets the memory alignment, typically aligned with external process pointer size
        /// </summary>
        private Int32 Alignment { get; set; }

        /// <summary>
        /// Gets or sets the time since the last read was performed on this region
        /// </summary>
        private DateTime TimeSinceLastRead { get; set; }

        /// <summary>
        /// Gets or sets the reference to the snapshot element being iterated over
        /// </summary>
        private ISnapshotElementRef<DataType, LabelType> SnapshotElementRef { get; set; }

        public NewSnapshotRegion() : this(IntPtr.Zero, 0)
        {
        }

        public NewSnapshotRegion(NormalizedRegion normalizedRegion) : this(normalizedRegion == null ? IntPtr.Zero :
            normalizedRegion.BaseAddress, normalizedRegion == null ? 0 : normalizedRegion.RegionSize)
        {
        }

        public NewSnapshotRegion(IntPtr baseAddress, Int32 regionSize) : base(baseAddress, regionSize)
        {
            this.TimeSinceLastRead = DateTime.MinValue;
        }


        public ISnapshotElementRef this[Int32 index]
        {
            get
            {
                ISnapshotElementRef element = new SnapshotElementRef<DataType, LabelType>(this);
                element.InitializePointers(index);
                return element;
            }
        }

        public void SetAlignment(Int32 alignment)
        {
            this.Alignment = alignment;

            // Enforce alignment constraint on base address
            if (this.BaseAddress.Mod(alignment).ToUInt64() != 0)
            {
                unchecked
                {
                    this.BaseAddress = this.BaseAddress.Subtract(this.BaseAddress.Mod(alignment));
                    this.BaseAddress = this.BaseAddress.Add(alignment);

                    this.RegionSize -= alignment - this.BaseAddress.Mod(alignment).ToInt32();
                    if (this.RegionSize < 0)
                    {
                        this.RegionSize = 0;
                    }
                }
            }
        }

        public void SetAllValidBits(Boolean isValid)
        {
            this.ValidBits = new BitArray(this.RegionSize, isValid);
        }

        public void Relax(Int32 relaxSize)
        {
            // TODO: Rollovers
            this.BaseAddress = this.BaseAddress.Add(relaxSize);
            this.RegionSize -= relaxSize;
        }

        public void Expand(Int32 expandSize)
        {
            // TODO: Rollovers
            this.BaseAddress = this.BaseAddress.Subtract(expandSize);
            this.RegionSize += expandSize;
        }

        public void SetCurrentValues(Byte[] newValues)
        {
            this.CurrentValues = newValues;
        }

        public void SetPreviousValues(Byte[] newValues)
        {
            this.PreviousValues = newValues;
        }

        public void SetElementLabels(params LabelType[] newLabels)
        {
            this.ElementLabels = newLabels;
        }

        public BitArray GetValidBits()
        {
            return this.ValidBits;
        }

        public DateTime GetTimeSinceLastRead()
        {
            return this.TimeSinceLastRead;
        }

        public Boolean CanCompare()
        {
            if (this.PreviousValues == null || this.CurrentValues == null || this.PreviousValues.Length != this.CurrentValues.Length)
            {
                return false;
            }

            return true;
        }

        public Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true)
        {
            this.TimeSinceLastRead = DateTime.Now;

            readSuccess = false;
            Byte[] currentValues = EngineCore.GetInstance().OperatingSystemAdapter.ReadBytes(this.BaseAddress, this.RegionSize, out readSuccess);

            if (!readSuccess)
            {
                return null;
            }

            if (keepValues)
            {
                this.SetCurrentValues(currentValues);
            }

            return currentValues;
        }

        public Boolean ContainsAddress(IntPtr address)
        {
            // TODO
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
            // TODO
            return null;
        }

        public Int64 GetByteCount()
        {
            return this.CurrentValues == null ? 0L : this.CurrentValues.LongLength;
        }

        public Int32 GetElementCount()
        {
            return unchecked((Int32)(this.CurrentValues == null ? 0L : this.CurrentValues.LongLength / this.Alignment));
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

        public IntPtr GetBaseAddress()
        {
            return this.BaseAddress;
        }
    }
    //// End class
}
//// End namespace