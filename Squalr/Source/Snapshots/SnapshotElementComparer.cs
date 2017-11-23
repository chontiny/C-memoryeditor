namespace Squalr.Source.Snapshots
{
    using Scanners.ScanConstraints;
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines a reference to an element within a snapshot region.
    /// </summary>
    internal class SnapshotElementComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotElementComparer" /> class.
        /// </summary>
        /// <param name="parent">The parent region that contains this element.</param>
        /// <param name="elementIndex">The index of the element to begin pointing to.</param>
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        /// <param name="pointerIncrementMode">The method by which to increment element pointers.</param>
        public unsafe SnapshotElementComparer(
            SnapshotRegion parent,
            Int32 elementIndex = 0,
            PointerIncrementMode pointerIncrementMode = PointerIncrementMode.AllPointers,
            ScanConstraint.ConstraintType compareActionConstraint = ScanConstraint.ConstraintType.Changed,
            Object compareActionValue = null)
        {
            this.Parent = parent;
            this.ElementIndex = elementIndex;

            this.InitializePointers(elementIndex);
            this.SetConstraintFunctions();
            this.SetPointerFunction(pointerIncrementMode);
            this.SetCompareAction(compareActionConstraint, compareActionValue);
        }

        /// <summary>
        /// Gets an action to increment only the needed pointers.
        /// </summary>
        public Action IncrementPointers { get; private set; }

        /// <summary>
        /// Gets an action based on the element iterator scan constraint.
        /// </summary>
        public Func<Boolean> Compare { get; private set; }

        /// <summary>
        /// Gets a function to load the current value.
        /// </summary>
        public Func<Object> LoadCurrentValue { get; private set; }

        /// <summary>
        /// Gets a function to load the previous value.
        /// </summary>
        public Func<Object> LoadPreviousValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has changed.
        /// </summary>
        public Func<Boolean> Changed { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has not changed.
        /// </summary>
        public Func<Boolean> Unchanged { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has increased.
        /// </summary>
        public Func<Boolean> Increased { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has decreased.
        /// </summary>
        public Func<Boolean> Decreased { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value equal to the given value.
        /// </summary>
        public Func<Object, Boolean> EqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value not equal to the given value.
        /// </summary>
        public Func<Object, Boolean> NotEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value greater than to the given value.
        /// </summary>
        public Func<Object, Boolean> GreaterThanValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value greater than or equal to the given value.
        /// </summary>
        public Func<Object, Boolean> GreaterThanOrEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value less than to the given value.
        /// </summary>
        public Func<Object, Boolean> LessThanValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value less than to the given value.
        /// </summary>
        public Func<Object, Boolean> LessThanOrEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the element has increased it's value by the given value.
        /// </summary>
        public Func<Object, Boolean> IncreasedByValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the element has decreased it's value by the given value.
        /// </summary>
        public Func<Object, Boolean> DecreasedByValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the value is in scientific notation. Only applicable for Single and Double data types.
        /// </summary>
        public Func<Boolean> IsScientificNotation { get; private set; }

        /// <summary>
        /// Gets the base address of this element.
        /// </summary>
        public IntPtr BaseAddress
        {
            get
            {
                return this.Parent.ReadGroup.BaseAddress.Add(this.Parent.ReadGroupOffset).Add(this.ElementIndex);
            }
        }

        /// <summary>
        /// Gets or sets the label associated with this element.
        /// </summary>
        public Object ElementLabel
        {
            get
            {
                return this.Parent.ReadGroup.ElementLabels[this.CurrentLabelIndex];
            }

            set
            {
                this.Parent.ReadGroup.ElementLabels[this.CurrentLabelIndex] = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent snapshot region.
        /// </summary>
        private SnapshotRegion Parent { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the current value.
        /// </summary>
        private unsafe Byte* CurrentValuePointer { get; set; }

        /// <summary>
        /// Gets or sets the pointer to the previous value.
        /// </summary>
        private unsafe Byte* PreviousValuePointer { get; set; }

        /// <summary>
        /// Gets or sets the index of this element, used for setting and getting the label.
        /// Note that we cannot have a pointer to the label, as it is a non-blittable type.
        /// </summary>
        private Int32 CurrentLabelIndex { get; set; }

        /// <summary>
        /// Gets the index of this element.
        /// </summary>
        private unsafe Int32 ElementIndex { get; set; }

        /// <summary>
        /// Gets or sets the data type of this element.
        /// </summary>
        private DataType DataType { get; set; }

        /// <summary>
        /// Enums determining which pointers need to be updated every iteration.
        /// </summary>
        public enum PointerIncrementMode
        {
            /// <summary>
            /// Increment all pointers.
            /// </summary>
            AllPointers,

            /// <summary>
            /// Only increment current and previous value pointers.
            /// </summary>
            ValuesOnly,

            /// <summary>
            /// Only increment label pointers.
            /// </summary>
            LabelsOnly,

            /// <summary>
            /// Only increment current value pointer.
            /// </summary>
            CurrentOnly,

            /// <summary>
            /// Increment all pointers except the previous value pointer.
            /// </summary>
            NoPrevious,
        }

        /// <summary>
        /// Gets the label of this element.
        /// </summary>
        /// <returns>The label of this element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Object GetElementLabel()
        {
            return this.Parent.ReadGroup.ElementLabels == null ? null : this.Parent.ReadGroup.ElementLabels[this.CurrentLabelIndex];
        }

        /// <summary>
        /// Sets the label of this element.
        /// </summary>
        /// <param name="newLabel">The new element label.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetElementLabel(Object newLabel)
        {
            this.Parent.ReadGroup.ElementLabels[this.CurrentLabelIndex] = newLabel;
        }

        /// <summary>
        /// Determines if this element has a current value associated with it.
        /// </summary>
        /// <returns>True if a current value is present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean HasCurrentValue()
        {
            if (this.CurrentValuePointer == (Byte*)0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if this element has a previous value associated with it.
        /// </summary>
        /// <returns>True if a previous value is present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean HasPreviousValue()
        {
            if (this.PreviousValuePointer == (Byte*)0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes snapshot value reference pointers
        /// </summary>
        /// <param name="index">The index of the element to begin pointing to.</param>
        private unsafe void InitializePointers(Int32 index = 0)
        {
            this.CurrentLabelIndex = index;
            this.DataType = this.Parent.ReadGroup.ElementDataType;

            if (this.Parent?.ReadGroup?.CurrentValues != null && this.Parent.ReadGroupOffset + index < this.Parent.ReadGroup.CurrentValues.Length)
            {
                fixed (Byte* pointerBase = &this.Parent.ReadGroup.CurrentValues[this.Parent.ReadGroupOffset + index])
                {
                    this.CurrentValuePointer = pointerBase;
                }
            }
            else
            {
                this.CurrentValuePointer = null;
            }

            if (this.Parent?.ReadGroup?.PreviousValues != null && this.Parent.ReadGroupOffset + index < this.Parent.ReadGroup.PreviousValues.Length)
            {
                fixed (Byte* pointerBase = &this.Parent.ReadGroup.PreviousValues[this.Parent.ReadGroupOffset + index])
                {
                    this.PreviousValuePointer = pointerBase;
                }
            }
            else
            {
                this.PreviousValuePointer = null;
            }
        }

        /// <summary>
        /// Initializes all constraint functions for value comparisons.
        /// </summary>
        private unsafe void SetConstraintFunctions()
        {
            switch (this.DataType)
            {
                case DataType type when type == DataTypes.Byte:
                    this.LoadCurrentValue = () => { return *this.CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *this.PreviousValuePointer; };
                    this.Changed = () => { return *this.CurrentValuePointer != *this.PreviousValuePointer; };
                    this.Unchanged = () => { return *this.CurrentValuePointer == *this.PreviousValuePointer; };
                    this.Increased = () => { return *this.CurrentValuePointer > *this.PreviousValuePointer; };
                    this.Decreased = () => { return *this.CurrentValuePointer < *this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *this.CurrentValuePointer == (Byte)value; };
                    this.NotEqualToValue = (value) => { return *this.CurrentValuePointer != (Byte)value; };
                    this.GreaterThanValue = (value) => { return *this.CurrentValuePointer > (Byte)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *this.CurrentValuePointer >= (Byte)value; };
                    this.LessThanValue = (value) => { return *this.CurrentValuePointer < (Byte)value; };
                    this.LessThanOrEqualToValue = (value) => { return *this.CurrentValuePointer <= (Byte)value; };
                    this.IncreasedByValue = (value) => { return *this.CurrentValuePointer == unchecked(*this.PreviousValuePointer + (Byte)value); };
                    this.DecreasedByValue = (value) => { return *this.CurrentValuePointer == unchecked(*this.PreviousValuePointer - (Byte)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.SByte:
                    this.LoadCurrentValue = () => { return *(SByte*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(SByte*)PreviousValuePointer; };
                    this.Changed = () => { return *(SByte*)this.CurrentValuePointer != *(SByte*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(SByte*)this.CurrentValuePointer == *(SByte*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(SByte*)this.CurrentValuePointer > *(SByte*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(SByte*)this.CurrentValuePointer < *(SByte*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(SByte*)this.CurrentValuePointer == (SByte)value; };
                    this.NotEqualToValue = (value) => { return *(SByte*)this.CurrentValuePointer != (SByte)value; };
                    this.GreaterThanValue = (value) => { return *(SByte*)this.CurrentValuePointer > (SByte)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(SByte*)this.CurrentValuePointer >= (SByte)value; };
                    this.LessThanValue = (value) => { return *(SByte*)this.CurrentValuePointer < (SByte)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(SByte*)this.CurrentValuePointer <= (SByte)value; };
                    this.IncreasedByValue = (value) => { return *(SByte*)this.CurrentValuePointer == unchecked(*(SByte*)this.PreviousValuePointer + (SByte)value); };
                    this.DecreasedByValue = (value) => { return *(SByte*)this.CurrentValuePointer == unchecked(*(SByte*)this.PreviousValuePointer - (SByte)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.Int16:
                    this.LoadCurrentValue = () => { return *(Int16*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(Int16*)PreviousValuePointer; };
                    this.Changed = () => { return *(Int16*)this.CurrentValuePointer != *(Int16*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(Int16*)this.CurrentValuePointer == *(Int16*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(Int16*)this.CurrentValuePointer > *(Int16*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(Int16*)this.CurrentValuePointer < *(Int16*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(Int16*)this.CurrentValuePointer == (Int16)value; };
                    this.NotEqualToValue = (value) => { return *(Int16*)this.CurrentValuePointer != (Int16)value; };
                    this.GreaterThanValue = (value) => { return *(Int16*)this.CurrentValuePointer > (Int16)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(Int16*)this.CurrentValuePointer >= (Int16)value; };
                    this.LessThanValue = (value) => { return *(Int16*)this.CurrentValuePointer < (Int16)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(Int16*)this.CurrentValuePointer <= (Int16)value; };
                    this.IncreasedByValue = (value) => { return *(Int16*)this.CurrentValuePointer == unchecked(*(Int16*)this.PreviousValuePointer + (Int16)value); };
                    this.DecreasedByValue = (value) => { return *(Int16*)this.CurrentValuePointer == unchecked(*(Int16*)this.PreviousValuePointer - (Int16)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.Int32:
                    this.LoadCurrentValue = () => { return *(Int32*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(Int32*)PreviousValuePointer; };
                    this.Changed = () => { return *(Int32*)this.CurrentValuePointer != *(Int32*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(Int32*)this.CurrentValuePointer == *(Int32*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(Int32*)this.CurrentValuePointer > *(Int32*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(Int32*)this.CurrentValuePointer < *(Int32*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(Int32*)this.CurrentValuePointer == (Int32)value; };
                    this.NotEqualToValue = (value) => { return *(Int32*)this.CurrentValuePointer != (Int32)value; };
                    this.GreaterThanValue = (value) => { return *(Int32*)this.CurrentValuePointer > (Int32)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(Int32*)this.CurrentValuePointer >= (Int32)value; };
                    this.LessThanValue = (value) => { return *(Int32*)this.CurrentValuePointer < (Int32)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(Int32*)this.CurrentValuePointer <= (Int32)value; };
                    this.IncreasedByValue = (value) => { return *(Int32*)this.CurrentValuePointer == unchecked(*(Int32*)this.PreviousValuePointer + (Int32)value); };
                    this.DecreasedByValue = (value) => { return *(Int32*)this.CurrentValuePointer == unchecked(*(Int32*)this.PreviousValuePointer - (Int32)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.Int64:
                    this.LoadCurrentValue = () => { return *(Int64*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(Int64*)PreviousValuePointer; };
                    this.Changed = () => { return *(Int64*)this.CurrentValuePointer != *(Int64*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(Int64*)this.CurrentValuePointer == *(Int64*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(Int64*)this.CurrentValuePointer > *(Int64*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(Int64*)this.CurrentValuePointer < *(Int64*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(Int64*)this.CurrentValuePointer == (Int64)value; };
                    this.NotEqualToValue = (value) => { return *(Int64*)this.CurrentValuePointer != (Int64)value; };
                    this.GreaterThanValue = (value) => { return *(Int64*)this.CurrentValuePointer > (Int64)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(Int64*)this.CurrentValuePointer >= (Int64)value; };
                    this.LessThanValue = (value) => { return *(Int64*)this.CurrentValuePointer < (Int64)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(Int64*)this.CurrentValuePointer <= (Int64)value; };
                    this.IncreasedByValue = (value) => { return *(Int64*)this.CurrentValuePointer == unchecked(*(Int64*)this.PreviousValuePointer + (Int64)value); };
                    this.DecreasedByValue = (value) => { return *(Int64*)this.CurrentValuePointer == unchecked(*(Int64*)this.PreviousValuePointer - (Int64)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.UInt16:
                    this.LoadCurrentValue = () => { return *(UInt16*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(UInt16*)PreviousValuePointer; };
                    this.Changed = () => { return *(UInt16*)this.CurrentValuePointer != *(UInt16*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(UInt16*)this.CurrentValuePointer == *(UInt16*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(UInt16*)this.CurrentValuePointer > *(UInt16*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(UInt16*)this.CurrentValuePointer < *(UInt16*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(UInt16*)this.CurrentValuePointer == (UInt16)value; };
                    this.NotEqualToValue = (value) => { return *(UInt16*)this.CurrentValuePointer != (UInt16)value; };
                    this.GreaterThanValue = (value) => { return *(UInt16*)this.CurrentValuePointer > (UInt16)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(UInt16*)this.CurrentValuePointer >= (UInt16)value; };
                    this.LessThanValue = (value) => { return *(UInt16*)this.CurrentValuePointer < (UInt16)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(UInt16*)this.CurrentValuePointer <= (UInt16)value; };
                    this.IncreasedByValue = (value) => { return *(UInt16*)this.CurrentValuePointer == unchecked(*(UInt16*)this.PreviousValuePointer + (UInt16)value); };
                    this.DecreasedByValue = (value) => { return *(UInt16*)this.CurrentValuePointer == unchecked(*(UInt16*)this.PreviousValuePointer - (UInt16)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.UInt32:
                    this.LoadCurrentValue = () => { return *(UInt32*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(UInt32*)PreviousValuePointer; };
                    this.Changed = () => { return *(UInt32*)this.CurrentValuePointer != *(UInt32*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(UInt32*)this.CurrentValuePointer == *(UInt32*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(UInt32*)this.CurrentValuePointer > *(UInt32*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(UInt32*)this.CurrentValuePointer < *(UInt32*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(UInt32*)this.CurrentValuePointer == (UInt32)value; };
                    this.NotEqualToValue = (value) => { return *(UInt32*)this.CurrentValuePointer != (UInt32)value; };
                    this.GreaterThanValue = (value) => { return *(UInt32*)this.CurrentValuePointer > (UInt32)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(UInt32*)this.CurrentValuePointer >= (UInt32)value; };
                    this.LessThanValue = (value) => { return *(UInt32*)this.CurrentValuePointer < (UInt32)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(UInt32*)this.CurrentValuePointer <= (UInt32)value; };
                    this.IncreasedByValue = (value) => { return *(UInt32*)this.CurrentValuePointer == unchecked(*(UInt32*)this.PreviousValuePointer + (UInt32)value); };
                    this.DecreasedByValue = (value) => { return *(UInt32*)this.CurrentValuePointer == unchecked(*(UInt32*)this.PreviousValuePointer - (UInt32)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.UInt64:
                    this.LoadCurrentValue = () => { return *(UInt64*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(UInt64*)PreviousValuePointer; };
                    this.Changed = () => { return *(UInt64*)this.CurrentValuePointer != *(UInt64*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(UInt64*)this.CurrentValuePointer == *(UInt64*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(UInt64*)this.CurrentValuePointer > *(UInt64*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(UInt64*)this.CurrentValuePointer < *(UInt64*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return *(UInt64*)this.CurrentValuePointer == (UInt64)value; };
                    this.NotEqualToValue = (value) => { return *(UInt64*)this.CurrentValuePointer != (UInt64)value; };
                    this.GreaterThanValue = (value) => { return *(UInt64*)this.CurrentValuePointer > (UInt64)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(UInt64*)this.CurrentValuePointer >= (UInt64)value; };
                    this.LessThanValue = (value) => { return *(UInt64*)this.CurrentValuePointer < (UInt64)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(UInt64*)this.CurrentValuePointer <= (UInt64)value; };
                    this.IncreasedByValue = (value) => { return *(UInt64*)this.CurrentValuePointer == unchecked(*(UInt64*)this.PreviousValuePointer + (UInt64)value); };
                    this.DecreasedByValue = (value) => { return *(UInt64*)this.CurrentValuePointer == unchecked(*(UInt64*)this.PreviousValuePointer - (UInt64)value); };
                    this.IsScientificNotation = () => { return false; };
                    break;
                case DataType type when type == DataTypes.Single:
                    this.LoadCurrentValue = () => { return *(Single*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(Single*)PreviousValuePointer; };
                    this.Changed = () => { return *(Single*)this.CurrentValuePointer != *(Single*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(Single*)this.CurrentValuePointer == *(Single*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(Single*)this.CurrentValuePointer > *(Single*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(Single*)this.CurrentValuePointer < *(Single*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return (*(Single*)this.CurrentValuePointer).AlmostEquals((Single)value); };
                    this.NotEqualToValue = (value) => { return !(*(Single*)this.CurrentValuePointer).AlmostEquals((Single)value); };
                    this.GreaterThanValue = (value) => { return *(Single*)this.CurrentValuePointer > (Single)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(Single*)this.CurrentValuePointer >= (Single)value; };
                    this.LessThanValue = (value) => { return *(Single*)this.CurrentValuePointer < (Single)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(Single*)this.CurrentValuePointer <= (Single)value; };
                    this.IncreasedByValue = (value) => { return (*(Single*)this.CurrentValuePointer).AlmostEquals(unchecked(*(Single*)this.PreviousValuePointer + (Single)value)); };
                    this.DecreasedByValue = (value) => { return (*(Single*)this.CurrentValuePointer).AlmostEquals(unchecked(*(Single*)this.PreviousValuePointer - (Single)value)); };
                    this.IsScientificNotation = () => { return (*this.CurrentValuePointer).ToString().Contains("E"); };
                    break;
                case DataType type when type == DataTypes.Double:
                    this.LoadCurrentValue = () => { return *(Double*)CurrentValuePointer; };
                    this.LoadPreviousValue = () => { return *(Double*)PreviousValuePointer; };
                    this.Changed = () => { return *(Double*)this.CurrentValuePointer != *(Double*)this.PreviousValuePointer; };
                    this.Unchanged = () => { return *(Double*)this.CurrentValuePointer == *(Double*)this.PreviousValuePointer; };
                    this.Increased = () => { return *(Double*)this.CurrentValuePointer > *(Double*)this.PreviousValuePointer; };
                    this.Decreased = () => { return *(Double*)this.CurrentValuePointer < *(Double*)this.PreviousValuePointer; };
                    this.EqualToValue = (value) => { return (*(Double*)this.CurrentValuePointer).AlmostEquals((Double)value); };
                    this.NotEqualToValue = (value) => { return !(*(Double*)this.CurrentValuePointer).AlmostEquals((Double)value); };
                    this.GreaterThanValue = (value) => { return *(Double*)this.CurrentValuePointer > (Double)value; };
                    this.GreaterThanOrEqualToValue = (value) => { return *(Double*)this.CurrentValuePointer >= (Double)value; };
                    this.LessThanValue = (value) => { return *(Double*)this.CurrentValuePointer < (Double)value; };
                    this.LessThanOrEqualToValue = (value) => { return *(Double*)this.CurrentValuePointer <= (Double)value; };
                    this.IncreasedByValue = (value) => { return (*(Double*)this.CurrentValuePointer).AlmostEquals(unchecked(*(Double*)this.PreviousValuePointer + (Double)value)); };
                    this.DecreasedByValue = (value) => { return (*(Double*)this.CurrentValuePointer).AlmostEquals(unchecked(*(Double*)this.PreviousValuePointer - (Double)value)); };
                    this.IsScientificNotation = () => { return (*this.CurrentValuePointer).ToString().Contains("E"); };
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Initializes the pointer incrementing function based on the provided parameters.
        /// </summary>
        /// <param name="pointerIncrementMode">The method by which to increment pointers.</param>
        private unsafe void SetPointerFunction(PointerIncrementMode pointerIncrementMode)
        {
            Int32 alignment = this.Parent.ReadGroup.Alignment;

            if (alignment == 1)
            {
                switch (pointerIncrementMode)
                {
                    case PointerIncrementMode.AllPointers:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentLabelIndex++;
                            this.CurrentValuePointer++;
                            this.PreviousValuePointer++;
                        };
                        break;
                    case PointerIncrementMode.CurrentOnly:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentValuePointer++;
                        };
                        break;
                    case PointerIncrementMode.LabelsOnly:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentLabelIndex++;
                        };
                        break;
                    case PointerIncrementMode.NoPrevious:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentLabelIndex++;
                            this.CurrentValuePointer++;
                        };
                        break;
                    case PointerIncrementMode.ValuesOnly:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentValuePointer++;
                            this.PreviousValuePointer++;
                        };
                        break;
                }
            }
            else
            {
                switch (pointerIncrementMode)
                {
                    case PointerIncrementMode.AllPointers:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentLabelIndex += alignment;
                            this.CurrentValuePointer += alignment;
                            this.PreviousValuePointer += alignment;
                        };
                        break;
                    case PointerIncrementMode.CurrentOnly:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentValuePointer += alignment;
                        };
                        break;
                    case PointerIncrementMode.LabelsOnly:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentLabelIndex += alignment;
                        };
                        break;
                    case PointerIncrementMode.NoPrevious:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentLabelIndex += alignment;
                            this.CurrentValuePointer += alignment;
                        };
                        break;
                    case PointerIncrementMode.ValuesOnly:
                        this.IncrementPointers = () =>
                        {
                            this.CurrentValuePointer += alignment;
                            this.PreviousValuePointer += alignment;
                        };
                        break;
                }
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
                    this.Compare = this.Unchanged;
                    break;
                case ScanConstraint.ConstraintType.Changed:
                    this.Compare = this.Changed;
                    break;
                case ScanConstraint.ConstraintType.Increased:
                    this.Compare = this.Increased;
                    break;
                case ScanConstraint.ConstraintType.Decreased:
                    this.Compare = this.Decreased;
                    break;
                case ScanConstraint.ConstraintType.IncreasedByX:
                    this.Compare = () => this.IncreasedByValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.DecreasedByX:
                    this.Compare = () => this.DecreasedByValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.Equal:
                    this.Compare = () => this.EqualToValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.NotEqual:
                    this.Compare = () => this.NotEqualToValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.GreaterThan:
                    this.Compare = () => this.GreaterThanValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.GreaterThanOrEqual:
                    this.Compare = () => this.GreaterThanOrEqualToValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.LessThan:
                    this.Compare = () => this.LessThanValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.LessThanOrEqual:
                    this.Compare = () => this.LessThanValue(compareActionValue);
                    break;
                case ScanConstraint.ConstraintType.NotScientificNotation:
                    this.Compare = this.IsScientificNotation;
                    break;
            }
        }
    }
    //// End class
}
//// End namespace