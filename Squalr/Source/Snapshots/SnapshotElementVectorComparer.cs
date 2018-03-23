namespace Squalr.Source.Snapshots
{
    using Scanners.ScanConstraints;
    using Squalr.Engine;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Utils;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Class for comparing snapshot regions.
    /// </summary>
    internal class SnapshotElementVectorComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotElementVectorComparer" /> class.
        /// </summary>
        /// <param name="region">The parent region that contains this element.</param>
        /// <param name="vectorSize">The size of vectors on the system.</param>
        /// <param name="scanConstraints">The set of constraints to use for the element comparisons.</param>
        public unsafe SnapshotElementVectorComparer(
            SnapshotRegion region,
            ScanConstraintManager scanConstraints)
        {
            this.Region = region;
            this.VectorSize = Eng.GetInstance().Architecture.GetVectorSize();
            this.VectorReadBase = this.Region.ReadGroupOffset - this.Region.ReadGroupOffset % this.VectorSize;
            this.VectorReadIndex = 0;
            this.DataTypeize = Conversions.SizeOf(this.Region.ReadGroup.ElementDataType);

            // Initialize capacity to 1/16 elements
            this.ResultRegions = new List<SnapshotRegion>(unchecked((Int32)(this.Region.ElementCount)) / 16);

            this.SetConstraintFunctions();
            this.SetCompareAction(scanConstraints);
        }

        /// <summary>
        /// Gets an action based on the element iterator scan constraint.
        /// </summary>
        public Func<Vector<Byte>> VectorCompare { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has changed.
        /// </summary>
        public Func<Vector<Byte>> Changed { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has not changed.
        /// </summary>
        public Func<Vector<Byte>> Unchanged { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has increased.
        /// </summary>
        public Func<Vector<Byte>> Increased { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has decreased.
        /// </summary>
        public Func<Vector<Byte>> Decreased { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value equal to the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> EqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value not equal to the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> NotEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value greater than to the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> GreaterThanValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value greater than or equal to the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> GreaterThanOrEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value less than to the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> LessThanValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value less than to the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> LessThanOrEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the element has increased it's value by the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> IncreasedByValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the element has decreased it's value by the given value.
        /// </summary>
        public Func<Object, Vector<Byte>> DecreasedByValue { get; private set; }

        /// <summary>
        /// Gets or sets the parent snapshot region.
        /// </summary>
        private SnapshotRegion Region { get; set; }

        /// <summary>
        /// Gets or sets the index of this element.
        /// </summary>
        public Int32 VectorReadIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we are currently encoding a new result region.
        /// </summary>
        private Boolean Encoding { get; set; }

        /// <summary>
        /// Gets or sets the current run length for run length encoded current scan results.
        /// </summary>
        private Int32 RunLength { get; set; }

        /// <summary>
        /// Gets or sets the index of this element.
        /// </summary>
        private Int32 VectorReadBase { get; set; }

        /// <summary>
        /// Gets or sets the SSE vector size on the machine.
        /// </summary>
        private Int32 VectorSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the data type being compared.
        /// </summary>
        private Int32 DataTypeize { get; set; }

        /// <summary>
        /// Gets or sets the list of discovered result regions.
        /// </summary>
        private IList<SnapshotRegion> ResultRegions { get; set; }

        /// <summary>
        /// Compares the parent snapshot region at the current vector index.
        /// </summary>
        public IList<SnapshotRegion> Compare()
        {
            while (this.VectorReadIndex < this.Region.RegionSize)
            {
                Vector<Byte> scanResults = this.VectorCompare();

                // Check all vector results true (vector of 0xFF's, which is how SSE instructions store true)
                if (Vector.GreaterThanAll(scanResults, Vector<Byte>.Zero))
                {
                    this.RunLength += this.VectorSize;
                    this.Encoding = true;
                }
                // Check all vector results false
                else if (Vector.EqualsAll(scanResults, Vector<Byte>.Zero))
                {
                    if (this.Encoding)
                    {
                        this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.VectorReadBase + this.VectorReadIndex - this.RunLength, this.RunLength));
                        this.RunLength = 0;
                        this.Encoding = false;
                    }
                }
                // Otherwise the vector contains a mixture of true and false
                else
                {
                    for (Int32 index = 0; index < this.VectorSize; index += this.DataTypeize)
                    {
                        // Vector result was false
                        if (scanResults[unchecked((Int32)index)] == 0)
                        {
                            if (this.Encoding)
                            {
                                this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.VectorReadBase + this.VectorReadIndex + index - this.RunLength, this.RunLength));
                                this.RunLength = 0;
                                this.Encoding = false;
                            }
                        }
                        // Vector result was true
                        else
                        {
                            this.RunLength += this.DataTypeize;
                            this.Encoding = true;
                        }
                    }
                }

                this.VectorReadIndex += this.VectorSize;
            }

            return this.GatherCollectedRegions();
        }

        /// <summary>
        /// Finalizes any leftover snapshot regions and returns them.
        /// </summary>
        public IList<SnapshotRegion> GatherCollectedRegions()
        {
            // Create the final region if we are still encoding
            if (this.Encoding)
            {
                this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.VectorReadBase + this.VectorReadIndex - this.RunLength, this.RunLength));
                this.RunLength = 0;
                this.Encoding = false;
            }

            // Remove vector misaligned leading regions
            SnapshotRegion firstRegion = this.ResultRegions.FirstOrDefault();
            Int32 adjustedIndex = 0;

            while (firstRegion != null)
            {
                // Exit if not misaligned
                if (firstRegion.ReadGroupOffset >= this.Region.ReadGroupOffset)
                {
                    break;
                }

                Int32 misalignment = this.Region.ReadGroupOffset - firstRegion.ReadGroupOffset;

                if (firstRegion.RegionSize <= misalignment)
                {
                    // The region is totally eclipsed -- remove it
                    this.ResultRegions.Remove(firstRegion);
                }
                else
                {
                    // The region is partially eclipsed -- adjust the size
                    firstRegion.ReadGroupOffset += misalignment;
                    firstRegion.RegionSize -= misalignment;
                    adjustedIndex++;
                }

                firstRegion = this.ResultRegions.Skip(adjustedIndex).FirstOrDefault();
            }

            // Remove vector overreading trailing regions
            SnapshotRegion lastRegion = this.ResultRegions.LastOrDefault();
            adjustedIndex = 0;

            while (lastRegion != null)
            {
                // Exit if not overreading
                if (lastRegion.EndAddress.ToUInt64() <= this.Region.EndAddress.ToUInt64())
                {
                    break;
                }

                Int32 overread = (lastRegion.EndAddress.ToUInt64() - this.Region.EndAddress.ToUInt64()).ToInt32();

                if (lastRegion.RegionSize <= overread)
                {
                    // The region is totally eclipsed -- remove it
                    this.ResultRegions.Remove(lastRegion);
                }
                else
                {
                    lastRegion.RegionSize -= overread;
                    adjustedIndex++;
                }

                lastRegion = this.ResultRegions.Reverse().Skip(adjustedIndex).FirstOrDefault();
            }

            return this.ResultRegions;
        }

        /// <summary>
        /// Gets the current values at the current vector read index.
        /// </summary>
        private Vector<Byte> CurrentValues
        {
            get
            {
                return new Vector<Byte>(this.Region.ReadGroup.CurrentValues, unchecked((Int32)(this.VectorReadBase + this.VectorReadIndex)));
            }
        }

        /// <summary>
        /// Gets the previous values at the current vector read index.
        /// </summary>
        private Vector<Byte> PreviousValues
        {
            get
            {
                return new Vector<Byte>(this.Region.ReadGroup.PreviousValues, unchecked((Int32)(this.VectorReadBase + this.VectorReadIndex)));
            }
        }

        /// <summary>
        /// Initializes all constraint functions for value comparisons.
        /// </summary>
        private unsafe void SetConstraintFunctions()
        {
            switch (this.Region.ReadGroup.ElementDataType)
            {
                case DataType type when type == DataType.Byte:
                    this.Changed = () => { return Vector.Equals(this.CurrentValues, this.PreviousValues); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.Equals(this.CurrentValues, this.PreviousValues)); };
                    this.Increased = () => { return Vector.GreaterThan(this.CurrentValues, this.PreviousValues); };
                    this.Decreased = () => { return Vector.LessThan(this.CurrentValues, this.PreviousValues); };
                    this.EqualToValue = (value) => { return Vector.Equals(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.Equals(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.GreaterThan(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.GreaterThanOrEqual(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.LessThanValue = (value) => { return Vector.LessThan(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.LessThanOrEqual(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.IncreasedByValue = (value) => { return Vector.Equals(this.CurrentValues, Vector.Add(this.PreviousValues, new Vector<Byte>(unchecked((Byte)value)))); };
                    this.DecreasedByValue = (value) => { return Vector.Equals(this.CurrentValues, Vector.Subtract(this.PreviousValues, new Vector<Byte>(unchecked((Byte)value)))); };
                    break;
                case DataType type when type == DataType.SByte:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.Add(Vector.AsVectorSByte(this.PreviousValues), new Vector<SByte>(unchecked((SByte)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.Subtract(Vector.AsVectorSByte(this.PreviousValues), new Vector<SByte>(unchecked((SByte)value))))); };
                    break;
                case DataType type when type == DataType.Int16:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.Add(Vector.AsVectorInt16(this.PreviousValues), new Vector<Int16>(unchecked((Int16)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.Subtract(Vector.AsVectorInt16(this.PreviousValues), new Vector<Int16>(unchecked((Int16)value))))); };
                    break;
                case DataType type when type == DataType.Int32:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.Add(Vector.AsVectorInt32(this.PreviousValues), new Vector<Int32>(unchecked((Int32)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.Subtract(Vector.AsVectorInt32(this.PreviousValues), new Vector<Int32>(unchecked((Int32)value))))); };
                    break;
                case DataType type when type == DataType.Int64:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.Add(Vector.AsVectorInt64(this.PreviousValues), new Vector<Int64>(unchecked((Int64)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.Subtract(Vector.AsVectorInt64(this.PreviousValues), new Vector<Int64>(unchecked((Int64)value))))); };
                    break;
                case DataType type when type == DataType.UInt16:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.Add(Vector.AsVectorUInt16(this.PreviousValues), new Vector<UInt16>(unchecked((UInt16)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.Subtract(Vector.AsVectorUInt16(this.PreviousValues), new Vector<UInt16>(unchecked((UInt16)value))))); };
                    break;
                case DataType type when type == DataType.UInt32:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.Add(Vector.AsVectorUInt32(this.PreviousValues), new Vector<UInt32>(unchecked((UInt32)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.Subtract(Vector.AsVectorUInt32(this.PreviousValues), new Vector<UInt32>(unchecked((UInt32)value))))); };
                    break;
                case DataType type when type == DataType.UInt64:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.Add(Vector.AsVectorUInt64(this.PreviousValues), new Vector<UInt64>(unchecked((UInt64)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.Subtract(Vector.AsVectorUInt64(this.PreviousValues), new Vector<UInt64>(unchecked((UInt64)value))))); };
                    break;
                case DataType type when type == DataType.Single:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.Add(Vector.AsVectorSingle(this.PreviousValues), new Vector<Single>(unchecked((Single)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.Subtract(Vector.AsVectorSingle(this.PreviousValues), new Vector<Single>(unchecked((Single)value))))); };
                    break;
                case DataType type when type == DataType.Double:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues)))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.OnesComplement(Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value))))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), Vector.Add(Vector.AsVectorDouble(this.PreviousValues), new Vector<Double>(unchecked((Double)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), Vector.Subtract(Vector.AsVectorDouble(this.PreviousValues), new Vector<Double>(unchecked((Double)value))))); };
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Sets the default compare action to use for this element.
        /// </summary>
        /// <param name="scanConstraints">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        private void SetCompareAction(ScanConstraintManager scanConstraints)
        {
            foreach (ScanConstraint scanConstraint in scanConstraints)
            {
                Func<Vector<Byte>> vectorCompare = null;

                switch (scanConstraint.Constraint)
                {
                    case ScanConstraint.ConstraintType.Unchanged:
                        vectorCompare = this.Unchanged;
                        break;
                    case ScanConstraint.ConstraintType.Changed:
                        vectorCompare = this.Changed;
                        break;
                    case ScanConstraint.ConstraintType.Increased:
                        vectorCompare = this.Increased;
                        break;
                    case ScanConstraint.ConstraintType.Decreased:
                        vectorCompare = this.Decreased;
                        break;
                    case ScanConstraint.ConstraintType.IncreasedByX:
                        vectorCompare = () => this.IncreasedByValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.DecreasedByX:
                        vectorCompare = () => this.DecreasedByValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.Equal:
                        vectorCompare = () => this.EqualToValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.NotEqual:
                        vectorCompare = () => this.NotEqualToValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.GreaterThan:
                        vectorCompare = () => this.GreaterThanValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.GreaterThanOrEqual:
                        vectorCompare = () => this.GreaterThanOrEqualToValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.LessThan:
                        vectorCompare = () => this.LessThanValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.LessThanOrEqual:
                        vectorCompare = () => this.LessThanOrEqualToValue(scanConstraint.ConstraintValue);
                        break;
                    default:
                        throw new Exception("Unknown constraint type");
                }

                if (this.VectorCompare == null)
                {
                    this.VectorCompare = vectorCompare;
                }
                else
                {
                    // We have multiple constraints -- the best way to enforce this is to simply AND the scan results together
                    Func<Vector<Byte>> currentCompare = this.VectorCompare;
                    this.VectorCompare = () => Vector.BitwiseAnd(currentCompare.Invoke(), vectorCompare.Invoke());
                }
            }
        }
    }
    //// End class
}
//// End namespace