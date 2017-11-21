namespace Squalr.Source.Snapshots
{
    using Scanners.ScanConstraints;
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// Class for comparing snapshot regions.
    /// </summary>
    internal class SnapshotRegionComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotRegionComparer" /> class.
        /// </summary>
        /// <param name="parent">The parent region that contains this element.</param>
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        /// <param name="pointerIncrementMode">The method by which to increment element pointers.</param>
        public unsafe SnapshotRegionComparer(
            SnapshotRegion parent,
            Int32 vectorSize,
            ScanConstraint.ConstraintType compareActionConstraint,
            Object compareActionValue)
        {
            this.Parent = parent;
            this.VectorSize = vectorSize;
            this.ResultRegions = new List<SnapshotRegion>();

            this.SetConstraintFunctions();
            this.SetCompareAction(compareActionConstraint, compareActionValue);
        }

        private Boolean Encoding { get; set; }

        private Int32 RunLength { get; set; }

        private Int32 VectorSize { get; set; }

        public IList<SnapshotRegion> ResultRegions { get; set; }

        /// <summary>
        /// Gets an action based on the element iterator scan constraint.
        /// </summary>
        public Func<Vector<Byte>> VectorCompare { get; private set; }

        /// <summary>
        /// Gets a function to load the current value.
        /// </summary>
        public Func<Vector<Byte>> LoadCurrentValue { get; private set; }

        /// <summary>
        /// Gets a function to load the previous value.
        /// </summary>
        public Func<Vector<Byte>> LoadPreviousValue { get; private set; }

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
        /// Gets a function which determines if the value is in scientific notation. Only applicable for Single and Double data types.
        /// </summary>
        public Func<Vector<Byte>> IsScientificNotation { get; private set; }

        /// <summary>
        /// Gets the base address of this element.
        /// </summary>
        public IntPtr BaseAddress
        {
            get
            {
                return this.Parent.BaseAddress.Add(this.ElementIndex);
            }
        }

        /// <summary>
        /// Gets or sets the parent snapshot region.
        /// </summary>
        private SnapshotRegion Parent { get; set; }

        public void Compare()
        {
            Vector<Byte> scanResults = this.VectorCompare();

            // Check all true
            if (Vector.GreaterThanAll(scanResults, Vector<Byte>.Zero))
            {
                this.RunLength += this.VectorSize;

                this.Encoding = true;
            }
            // Check all false
            else if (Vector.EqualsAll(scanResults, Vector<Byte>.Zero))
            {
                if (this.Encoding)
                {
                    this.CreateSnapshot();
                    this.RunLength = 0;
                }
                else
                {
                    this.RunLength += this.VectorSize;
                }

                this.Encoding = false;
            }
            // Mixed and matching
            else
            {
                // Well, it isn't going to be easy

                this.RunLength += this.VectorSize;

                this.Encoding = false;
            }
        }

        private void CreateSnapshot()
        {
            this.ResultRegions.Add(new SnapshotRegion(this.Parent.ReadGroup, this.Parent.BaseAddress.Add(this.ElementIndex), this.RunLength.ToUInt64()));
        }

        private Vector<Byte> CurrentValues
        {
            get
            {
                return new Vector<Byte>(this.Parent.ReadGroup.CurrentValues, this.Parent.ReadGroupOffset + this.ElementIndex);
            }
        }

        private Vector<Byte> PreviousValues
        {
            get
            {
                return new Vector<Byte>(this.Parent.ReadGroup.PreviousValues, this.Parent.ReadGroupOffset + this.ElementIndex);
            }
        }

        /// <summary>
        /// Gets or sets the index of this element.
        /// </summary>
        public unsafe Int32 ElementIndex { get; set; }

        /// <summary>
        /// Initializes snapshot value reference pointers
        /// </summary>
        /// <param name="index">The index of the element to begin pointing to.</param>
        private unsafe void InitializePointers(Int32 index = 0)
        {
            this.ElementIndex = index;
        }

        /// <summary>
        /// Initializes all constraint functions for value comparisons.
        /// </summary>
        private unsafe void SetConstraintFunctions()
        {
            switch (this.Parent.ElementDataType)
            {
                case DataType type when type == DataTypes.Byte:
                    this.Changed = () => { return Vector.Equals(this.CurrentValues, this.PreviousValues); };
                    this.Unchanged = () => { return Vector.Equals(this.CurrentValues, this.PreviousValues); };
                    this.Increased = () => { return Vector.GreaterThan(this.CurrentValues, this.PreviousValues); };
                    this.Decreased = () => { return Vector.LessThan(this.CurrentValues, this.PreviousValues); };
                    this.EqualToValue = (value) => { return Vector.Equals(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.NotEqualToValue = (value) => { return Vector.Equals(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.GreaterThanValue = (value) => { return Vector.GreaterThan(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.GreaterThanOrEqual(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.LessThanValue = (value) => { return Vector.LessThan(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.LessThanOrEqual(this.CurrentValues, new Vector<Byte>(unchecked((Byte)value))); };
                    this.IncreasedByValue = (value) => { return Vector.Equals(this.CurrentValues, Vector.Add(this.PreviousValues, new Vector<Byte>(unchecked((Byte)value)))); };
                    this.DecreasedByValue = (value) => { return Vector.Equals(this.CurrentValues, Vector.Subtract(this.PreviousValues, new Vector<Byte>(unchecked((Byte)value)))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.SByte:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSByte(this.CurrentValues), Vector.AsVectorSByte(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorSByte(this.CurrentValues), new Vector<SByte>(unchecked((SByte)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.Add(Vector.AsVectorSByte(this.PreviousValues), new Vector<SByte>(unchecked((SByte)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSByte(this.CurrentValues), Vector.Subtract(Vector.AsVectorSByte(this.PreviousValues), new Vector<SByte>(unchecked((SByte)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.Int16:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt16(this.CurrentValues), Vector.AsVectorInt16(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorInt16(this.CurrentValues), new Vector<Int16>(unchecked((Int16)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.Add(Vector.AsVectorInt16(this.PreviousValues), new Vector<Int16>(unchecked((Int16)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt16(this.CurrentValues), Vector.Subtract(Vector.AsVectorInt16(this.PreviousValues), new Vector<Int16>(unchecked((Int16)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.Int32:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt32(this.CurrentValues), Vector.AsVectorInt32(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorInt32(this.CurrentValues), new Vector<Int32>(unchecked((Int32)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.Add(Vector.AsVectorInt32(this.PreviousValues), new Vector<Int32>(unchecked((Int32)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt32(this.CurrentValues), Vector.Subtract(Vector.AsVectorInt32(this.PreviousValues), new Vector<Int32>(unchecked((Int32)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.Int64:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt64(this.CurrentValues), Vector.AsVectorInt64(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorInt64(this.CurrentValues), new Vector<Int64>(unchecked((Int64)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.Add(Vector.AsVectorInt64(this.PreviousValues), new Vector<Int64>(unchecked((Int64)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorInt64(this.CurrentValues), Vector.Subtract(Vector.AsVectorInt64(this.PreviousValues), new Vector<Int64>(unchecked((Int64)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.UInt16:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt16(this.CurrentValues), Vector.AsVectorUInt16(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorUInt16(this.CurrentValues), new Vector<UInt16>(unchecked((UInt16)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.Add(Vector.AsVectorUInt16(this.PreviousValues), new Vector<UInt16>(unchecked((UInt16)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt16(this.CurrentValues), Vector.Subtract(Vector.AsVectorUInt16(this.PreviousValues), new Vector<UInt16>(unchecked((UInt16)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.UInt32:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt32(this.CurrentValues), Vector.AsVectorUInt32(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorUInt32(this.CurrentValues), new Vector<UInt32>(unchecked((UInt32)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.Add(Vector.AsVectorUInt32(this.PreviousValues), new Vector<UInt32>(unchecked((UInt32)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt32(this.CurrentValues), Vector.Subtract(Vector.AsVectorUInt32(this.PreviousValues), new Vector<UInt32>(unchecked((UInt32)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.UInt64:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt64(this.CurrentValues), Vector.AsVectorUInt64(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorUInt64(this.CurrentValues), new Vector<UInt64>(unchecked((UInt64)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.Add(Vector.AsVectorUInt64(this.PreviousValues), new Vector<UInt64>(unchecked((UInt64)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorUInt64(this.CurrentValues), Vector.Subtract(Vector.AsVectorUInt64(this.PreviousValues), new Vector<UInt64>(unchecked((UInt64)value))))); };
                    this.IsScientificNotation = () => { return new Vector<Byte>(); };
                    break;
                case DataType type when type == DataTypes.Single:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSingle(this.CurrentValues), Vector.AsVectorSingle(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.GreaterThanValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.GreaterThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.GreaterThanOrEqual(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.LessThanValue = (value) => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.LessThanOrEqualToValue = (value) => { return Vector.AsVectorByte(Vector.LessThanOrEqual(Vector.AsVectorSingle(this.CurrentValues), new Vector<Single>(unchecked((Single)value)))); };
                    this.IncreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.Add(Vector.AsVectorSingle(this.PreviousValues), new Vector<Single>(unchecked((Single)value))))); };
                    this.DecreasedByValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorSingle(this.CurrentValues), Vector.Subtract(Vector.AsVectorSingle(this.PreviousValues), new Vector<Single>(unchecked((Single)value))))); };
                    break;
                case DataType type when type == DataTypes.Double:
                    this.Changed = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.Unchanged = () => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.Increased = () => { return Vector.AsVectorByte(Vector.GreaterThan(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.Decreased = () => { return Vector.AsVectorByte(Vector.LessThan(Vector.AsVectorDouble(this.CurrentValues), Vector.AsVectorDouble(this.PreviousValues))); };
                    this.EqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
                    this.NotEqualToValue = (value) => { return Vector.AsVectorByte(Vector.Equals(Vector.AsVectorDouble(this.CurrentValues), new Vector<Double>(unchecked((Double)value)))); };
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
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        private void SetCompareAction(ScanConstraint.ConstraintType compareActionConstraint, Object compareActionValue)
        {
            switch (compareActionConstraint)
            {
                case ScanConstraint.ConstraintType.Unchanged:
                    this.VectorCompare = this.Unchanged;
                    break;
                case ScanConstraint.ConstraintType.Changed:
                    this.VectorCompare = this.Changed;
                    break;
                case ScanConstraint.ConstraintType.Increased:
                    this.VectorCompare = this.Increased;
                    break;
                case ScanConstraint.ConstraintType.Decreased:
                    this.VectorCompare = this.Decreased;
                    break;
                case ScanConstraint.ConstraintType.IncreasedByX:
                    this.VectorCompare = () => this.IncreasedByValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.DecreasedByX:
                    this.VectorCompare = () => this.DecreasedByValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.Equal:
                    this.VectorCompare = () => this.EqualToValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.NotEqual:
                    this.VectorCompare = () => this.NotEqualToValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.GreaterThan:
                    this.VectorCompare = () => this.GreaterThanValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.GreaterThanOrEqual:
                    this.VectorCompare = () => this.GreaterThanOrEqualToValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.LessThan:
                    this.VectorCompare = () => this.LessThanValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.LessThanOrEqual:
                    this.VectorCompare = () => this.LessThanValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.NotScientificNotation:
                    this.VectorCompare = this.IsScientificNotation;
                    break;
            }
        }
    }
    //// End class
}
//// End namespace