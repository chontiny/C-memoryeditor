namespace Ana.Source.Snapshots.Deprecating
{
    using Engine;
    using Engine.OperatingSystems;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Utils.Extensions;

    /// <summary>
    /// Defines a snapshot of memory in an external process.
    /// </summary>
    internal abstract class SnapshotRegionDeprecating : NormalizedRegion, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionDeprecating" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of the region</param>
        /// <param name="regionSize">The size of the region</param>
        public SnapshotRegionDeprecating(IntPtr baseAddress, Int32 regionSize) : base(baseAddress, regionSize)
        {
            this.RegionExtension = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionDeprecating" /> class.
        /// </summary>
        /// <param name="remoteRegion">The snapshot region of which this will clone the size and base address</param>
        public SnapshotRegionDeprecating(NormalizedRegion remoteRegion) : base(remoteRegion.BaseAddress, remoteRegion.RegionSize)
        {
            this.RegionExtension = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionDeprecating" /> class.
        /// </summary>
        /// <param name="snapshotRegion">The snapshot region of which this will clone the size, extension size, and base address</param>
        public SnapshotRegionDeprecating(SnapshotRegionDeprecating snapshotRegion) : base(snapshotRegion.BaseAddress, snapshotRegion.RegionSize)
        {
            this.RegionExtension = snapshotRegion.RegionExtension;
        }

        /// <summary>
        /// Gets or sets the element type of this region
        /// [Public as an access optimization, otherwise use getters and setters] 
        /// </summary>
        public Type ElementType { get; set; }

        /// <summary>
        /// Gets or sets the valid bits for use in filtering scans
        /// [Public as an access optimization, otherwise use getters and setters]
        /// </summary>
        public BitArray Valid { get; set; }

        /// <summary>
        /// Gets or sets the regions only access one element at a time, so it is held here to avoid uneccessary memory usage
        /// [Public as an access optimization, otherwise use getters and setters]
        /// </summary>
        public SnapshotElementDeprecating CurrentSnapshotElement { get; set; }

        /// <summary>
        /// Gets the time since the last read was performed on this region
        /// </summary>
        public DateTime TimeSinceLastRead { get; private set; }

        /// <summary>
        /// Gets or sets a value indicatating a safe number of read-over bytes
        /// </summary>
        protected Int32 RegionExtension { get; set; }

        /// <summary>
        /// Gets or sets the most recently read values
        /// </summary>
        protected unsafe Byte[] CurrentValues { get; set; }

        /// <summary>
        /// Gets or sets the previously read values
        /// </summary>
        protected unsafe Byte[] PreviousValues { get; set; }

        /// <summary>
        /// Gets or sets the memory alignment, typically aligned with native platform word size
        /// </summary>
        protected Int32 Alignment { get; set; }

        /// <summary>
        /// Indexes into the snapshot region, getting the snapshot element at the given location.
        /// </summary>
        /// <param name="index">The index of the snapshot element.</param>
        /// <returns>The snapshot element at the specified location.</returns>
        public unsafe abstract SnapshotElementDeprecating this[Int32 index] { get; }

        /// <summary>
        /// Expands a region by the element type size in both directions unconditionally.
        /// </summary>
        public void ExpandRegion()
        {
            this.ExpandRegion(this.GetElementReadOverSize());
        }

        /// <summary>
        /// Expands a region by a given size in both directions unconditionally.
        /// </summary>
        /// <param name="expandSize">The size of region expansion</param>
        public void ExpandRegion(Int32 expandSize)
        {
            // Expand with negative overflow protection
            if (this.BaseAddress.ToUInt64() > checked((UInt32)expandSize))
            {
                this.BaseAddress = this.BaseAddress.Subtract(expandSize);
            }
            else
            {
                this.BaseAddress = IntPtr.Zero;
            }

            // Expand with overflow protection
            Int32 newRegionSize = unchecked(this.RegionSize + (expandSize * 2));
            this.RegionSize = Math.Max(this.RegionSize, newRegionSize);
        }

        /// <summary>
        /// Fills a region into the available extension space.
        /// </summary>
        public void FillRegion()
        {
            Int32 expandSize = this.RegionExtension;
            this.RegionExtension = 0;

            // Expand with overflow protection
            Int32 newRegionSize = unchecked(this.RegionSize + expandSize);
            this.RegionSize = Math.Max(this.RegionSize, newRegionSize);
        }

        /// <summary>
        /// Shrinks a region by the current element size. The old space is marked as extension space.
        /// This is important for elements with values that span several bytes, which need to read past the region size.
        /// </summary>
        public void RelaxRegion()
        {
            Int32 relaxSize = this.GetElementReadOverSize();

            this.FillRegion();

            if (this.RegionSize >= relaxSize)
            {
                this.RegionSize -= relaxSize;
                this.RegionExtension = relaxSize;
            }
            else
            {
                this.RegionExtension = this.RegionSize;
                this.RegionSize = 0;
            }
        }

        /// <summary>
        /// Sets the valid bit of all elements in this snapshot to true.
        /// </summary>
        public void MarkAllValid()
        {
            this.Valid = new BitArray(this.RegionSize, true);
        }

        /// <summary>
        /// Sets the valid bit of all elements in this snapshot to false.
        /// </summary>
        public void MarkAllInvalid()
        {
            this.Valid = new BitArray(this.RegionSize, false);
        }

        /// <summary>
        /// Sets the element type of this region
        /// </summary>
        /// <param name="elementType">The new element type</param>
        public void SetElementType(Type elementType)
        {
            this.ElementType = elementType;

            if (elementType == null)
            {
                return;
            }

            // Adjust extension space accordingly
            this.RelaxRegion();
        }

        /// <summary>
        /// Sets the memory alignment of this snapshot region
        /// </summary>
        /// <param name="alignment">The new alignment</param>
        public void SetAlignment(Int32 alignment)
        {
            this.Alignment = alignment;

            if (alignment == 0)
            {
                return;
            }

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

        /// <summary>
        /// Sets the current values of this region
        /// </summary>
        /// <param name="newValues">The new values</param>
        public void SetCurrentValues(Byte[] newValues)
        {
            this.PreviousValues = this.CurrentValues;
            this.CurrentValues = newValues;
            this.CurrentSnapshotElement.InitializePointers();
        }

        /// <summary>
        /// Sets the previous values of this region
        /// </summary>
        /// <param name="previousValues">The new previous values</param>
        public void SetPreviousValues(Byte[] previousValues)
        {
            this.PreviousValues = previousValues;
            this.CurrentSnapshotElement.InitializePointers();
        }

        /// <summary>
        /// Determines how many extra bytes an element will need to read to determine it's value
        /// </summary>
        /// <returns>The readover size of this region</returns>
        public Int32 GetElementReadOverSize()
        {
            return Marshal.SizeOf(this.ElementType) - 1;
        }

        /// <summary>
        /// Gets the region extension of this snapshot.
        /// </summary>
        /// <returns>The region extension of this snapshot.</returns>
        public Int32 GetRegionExtension()
        {
            return this.RegionExtension;
        }

        /// <summary>
        /// Gets the current values of this snapshot.
        /// </summary>
        /// <returns>The current values of this snapshot.</returns>
        public Byte[] GetCurrentValues()
        {
            return this.CurrentValues;
        }

        /// <summary>
        /// Gets the previous values of this snapshot.
        /// </summary>
        /// <returns>The previous values of this snapshot.</returns>
        public Byte[] GetPreviousValues()
        {
            return this.PreviousValues;
        }

        /// <summary>
        /// Gets the element type of this snapshot.
        /// </summary>
        /// <returns>The element type of this snapshot.</returns>
        public Type GetElementType()
        {
            return this.ElementType;
        }

        /// <summary>
        /// Gets the memory alignment of this snapshot.
        /// </summary>
        /// <returns>The memory alignment of this snapshot.</returns>
        public Int32 GetAlignment()
        {
            return this.Alignment;
        }

        /// <summary>
        /// Determines if the values of this region can be compared with itself, ie previous and current values are initialized
        /// </summary>
        /// <returns>Returns true if an region can be compared with itself</returns>
        public Boolean CanCompare()
        {
            if (this.PreviousValues == null || this.CurrentValues == null || this.PreviousValues.Length != this.CurrentValues.Length)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicating if there are any values read from memory for this snapshot.
        /// </summary>
        /// <returns>True if there are any read memory values.</returns>
        public Boolean HasValues()
        {
            if (this.CurrentValues == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads all memory bounded by this region
        /// </summary>
        /// <param name="readSuccess">Whether or not the read was successful</param>
        /// <param name="keepValues">Whether or not this region should retain the read values in memory</param>
        /// <returns>The read values from memory</returns>
        public Byte[] ReadAllRegionMemory(out Boolean readSuccess, Boolean keepValues = true)
        {
            this.TimeSinceLastRead = DateTime.Now;

            readSuccess = false;
            Byte[] currentValues = EngineCore.GetInstance().OperatingSystemAdapter.ReadBytes(this.BaseAddress, this.RegionSize + this.RegionExtension, out readSuccess);

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

        /// <summary>
        /// Gets a snapshot element enumerator. Note: Do not parallelize this, there is only one enumerator object as an optimization.
        /// </summary>
        /// <returns>The snapshot element enumerator.</returns>
        public IEnumerator GetEnumerator()
        {
            if (this.RegionSize <= 0 || this.Alignment <= 0)
            {
                yield break;
            }

            // Prevent the GC from moving buffers around
            GCHandle currentValuesHandle = GCHandle.Alloc(this.CurrentValues, GCHandleType.Pinned);
            GCHandle previousValuesHandle = GCHandle.Alloc(this.PreviousValues, GCHandleType.Pinned);

            this.CurrentSnapshotElement.InitializePointers();

            // Return the first element. This allows us to call IncrementPointers each loop unconditionally, with small speed gains.
            yield return this.CurrentSnapshotElement;

            if (this.Alignment == 1)
            {
                // Utilizes ++ operators; fast but we check every address
                for (Int32 index = 1; index < this.RegionSize; index++)
                {
                    this.CurrentSnapshotElement.IncrementPointers();
                    yield return this.CurrentSnapshotElement;
                }
            }
            else
            {
                // Utilizes += operators; faster because we access far less addresses
                for (Int32 index = this.Alignment; index < this.RegionSize; index += this.Alignment)
                {
                    this.CurrentSnapshotElement.AddPointers(this.Alignment);
                    yield return this.CurrentSnapshotElement;
                }
            }

            // Let the GC do what it wants now
            currentValuesHandle.Free();
            previousValuesHandle.Free();
        }
    }
    //// End class

    /// <summary>
    /// Defines a snapshot of memory in an external process, as well as assigned labels to this memory
    /// </summary>
    /// <typeparam name="LabelType">The label type of this snapshot region</typeparam>
    internal class SnapshotRegionDeprecating<LabelType> : SnapshotRegionDeprecating where LabelType : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionDeprecating {LabelType}" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of the region</param>
        /// <param name="regionSize">The size of the region</param>
        public SnapshotRegionDeprecating(IntPtr baseAddress, Int32 regionSize) : base(baseAddress, regionSize)
        {
            this.CurrentSnapshotElement = new SnapshotElementDeprecating<LabelType>(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionDeprecating {LabelType}" /> class.
        /// </summary>
        /// <param name="remoteRegion">The snapshot region of which this will clone the size and base address</param>
        public SnapshotRegionDeprecating(NormalizedRegion remoteRegion) : base(remoteRegion)
        {
            this.CurrentSnapshotElement = new SnapshotElementDeprecating<LabelType>(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionDeprecating {LabelType}" /> class.
        /// </summary>
        /// <param name="snapshotRegion">The snapshot region of which this will clone the size, extension size, and base address</param>
        public SnapshotRegionDeprecating(SnapshotRegionDeprecating snapshotRegion) : base(snapshotRegion)
        {
            this.CurrentSnapshotElement = new SnapshotElementDeprecating<LabelType>(this);
            this.SetCurrentValues(snapshotRegion.GetCurrentValues() == null ? null : (Byte[])snapshotRegion.GetCurrentValues().Clone());
            this.SetPreviousValues(snapshotRegion.GetPreviousValues() == null ? null : (Byte[])snapshotRegion.GetPreviousValues().Clone());
        }

        /// <summary>
        /// Gets or sets the labels for individual elements
        /// </summary>
        public LabelType?[] ElementLabels { get; set; }

        /// <summary>
        /// Indexer to access a labeled unified snapshot element at the specified index
        /// </summary>
        /// <param name="index">The index of the element</param>
        /// <returns>The snapshot at the specified index</returns>
        public unsafe override SnapshotElementDeprecating this[Int32 index]
        {
            get
            {
                SnapshotElementDeprecating<LabelType> element = new SnapshotElementDeprecating<LabelType>(this);
                element.InitializePointers(index);
                return element;
            }
        }

        /// <summary>
        /// Gets the element labels associated with this snapshot
        /// </summary>
        /// <returns>The element labels of this snapshot</returns>
        public LabelType?[] GetElementLabels()
        {
            return this.ElementLabels;
        }

        /// <summary>
        /// Sets the element label for this snapshot. Sets a single label for all elements at once.
        /// </summary>
        /// <param name="elementLabel">The new element label.</param>
        public void SetElementLabels(LabelType? elementLabel)
        {
            this.ElementLabels = Enumerable.Repeat(elementLabel, this.RegionSize).ToArray();
        }

        /// <summary>
        /// Sets the element labels for this snapshot.
        /// </summary>
        /// <param name="elementLabels">The new element labels.</param>
        public void SetElementLabels(LabelType?[] elementLabels)
        {
            this.ElementLabels = elementLabels;
        }

        /// <summary>
        /// Retrieves a subset of snapshot regions where valid bits are true. Valid bits will be cleared in the new regions
        /// </summary>
        /// <returns>A subset of the snapshot regions where the valid bits are true</returns>
        public IEnumerable<SnapshotRegionDeprecating<LabelType>> GetValidRegions()
        {
            List<SnapshotRegionDeprecating<LabelType>> validRegions = new List<SnapshotRegionDeprecating<LabelType>>();
            for (Int32 startIndex = 0; startIndex < (this.Valid == null ? 0 : this.Valid.Length); startIndex += this.Alignment)
            {
                if (!this.Valid[startIndex])
                {
                    continue;
                }

                // Get the length of this valid region
                Int32 validRegionSize = 0;
                do
                {
                    // We only care if the aligned elements are valid
                    validRegionSize += this.Alignment;
                }
                while (startIndex + validRegionSize < this.Valid.Length && this.Valid[startIndex + validRegionSize]);

                // Create new subregion from this valid region
                SnapshotRegionDeprecating<LabelType> subRegion = new SnapshotRegionDeprecating<LabelType>(this.BaseAddress + startIndex, validRegionSize);

                // Collect the current values. Attempts to collect out of bounds into extended space.
                subRegion.SetCurrentValues(this.CurrentValues.LargestSubArray(startIndex, validRegionSize + this.GetElementReadOverSize()));
                subRegion.SetPreviousValues(this.PreviousValues.LargestSubArray(startIndex, validRegionSize + this.GetElementReadOverSize()));

                // Collect the element labels. Attempts to collect out of bounds into extended space.
                subRegion.SetElementLabels(this.ElementLabels.LargestSubArray(startIndex, validRegionSize + this.GetElementReadOverSize()));

                // If we were able to grab values into the extended space, update the extension size.
                if (subRegion.GetCurrentValues() != null)
                {
                    subRegion.RegionExtension = subRegion.GetCurrentValues().Length - validRegionSize;
                }
                else
                {
                    subRegion.RegionExtension = this.GetElementReadOverSize();
                }

                subRegion.SetElementType(this.ElementType);
                subRegion.SetAlignment(this.Alignment);

                validRegions.Add(subRegion);

                startIndex += validRegionSize;
            }

            // Free memory for GC on valid array
            this.Valid = null;

            return validRegions;
        }
    }
    //// End class
}
//// End namespace