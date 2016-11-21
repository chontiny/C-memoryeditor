namespace Ana.Source.Snapshots
{
    using System;
    using System.Runtime.CompilerServices;
    using Utils.Extensions;

    /// <summary>
    /// Defines a reference to an element within a snapshot region.
    /// </summary>
    /// <typeparam name="DataType">The data type of this snapshot element.</typeparam>
    /// <typeparam name="LabelType">The type corresponding to the labels of this snapshot element.</typeparam>
    internal class SnapshotElementRef<DataType, LabelType> : ISnapshotElementRef<DataType, LabelType>
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotElementRef {DataType,LabelType}" /> class.
        /// </summary>
        /// <param name="parent">The parent region that contains this element</param>
        public SnapshotElementRef(ISnapshotRegion<DataType, LabelType> parent)
        {
            this.Parent = parent;
        }

        /// <summary>
        /// Gets or sets the parent snapshot region.
        /// </summary>
        private ISnapshotRegion<DataType, LabelType> Parent { get; set; }

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
        private Int32 CurrentElementIndex { get; set; }

        /// <summary>
        /// Gets or sets the type code associated with the data type of this element.
        /// </summary>
        private TypeCode CurrentTypeCode { get; set; }

        /// <summary>
        /// Initializes snapshot value reference pointers
        /// </summary>
        /// <param name="index">The index of the element to begin pointing to.</param>
        public unsafe void InitializePointers(Int32 index = 0)
        {
            this.CurrentElementIndex = index;
            this.CurrentTypeCode = Type.GetTypeCode(typeof(DataType));
            Byte[] currentValues = this.Parent.GetCurrentValues();
            Byte[] previousValues = this.Parent.GetPreviousValues();

            if (currentValues != null && currentValues.Length > 0)
            {
                fixed (Byte* pointerBase = &currentValues[index])
                {
                    this.CurrentValuePointer = pointerBase;
                }
            }
            else
            {
                this.CurrentValuePointer = null;
            }

            if (previousValues != null && previousValues.Length > 0)
            {
                fixed (Byte* pointerBase = &previousValues[index])
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
        /// Increments all value and label pointers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void IncrementPointers()
        {
            this.CurrentElementIndex++;
            this.CurrentValuePointer++;
            this.PreviousValuePointer++;
        }

        /// <summary>
        /// Increments all value and label pointers by the given alignment.
        /// </summary>
        /// <param name="alignment">The alignment by which to increment.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddPointers(Int32 alignment)
        {
            this.CurrentElementIndex += alignment;
            this.CurrentValuePointer += alignment;
            this.PreviousValuePointer += alignment;
        }

        /// <summary>
        /// Sets the valid bit of this element.
        /// </summary>
        /// <param name="isValid">Whether or not this element's valid bit is set.</param>
        public void SetValid(Boolean isValid)
        {
            this.Parent.GetValidBits().Set(this.CurrentElementIndex, isValid);
        }

        /// <summary>
        /// Gets the base address of this element in memory.
        /// </summary>
        /// <returns>The base address of this element in memory.</returns>
        public IntPtr GetBaseAddress()
        {
            return this.Parent.GetBaseAddress().Add(this.CurrentElementIndex);
        }

        /// <summary>
        /// Determines if this element has changed.
        /// </summary>
        /// <returns>True if the element changed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Changed()
        {
            return !this.LoadValue(this.CurrentValuePointer).Equals(this.LoadValue(this.PreviousValuePointer));
        }

        /// <summary>
        /// Determines if this element has not changed.
        /// </summary>
        /// <returns>True if the element is unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Unchanged()
        {
            return this.LoadValue(this.CurrentValuePointer).Equals(this.LoadValue(this.PreviousValuePointer));
        }

        /// <summary>
        /// Determines if this element has increased.
        /// </summary>
        /// <returns>True if the element increased.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Increased()
        {
            return this.LoadValue(this.CurrentValuePointer).CompareTo(this.LoadValue(this.PreviousValuePointer)) > 0;
        }

        /// <summary>
        /// Determines if this element has decreased.
        /// </summary>
        /// <returns>True if the element decreased.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Decreased()
        {
            return this.LoadValue(this.CurrentValuePointer).CompareTo(this.LoadValue(this.PreviousValuePointer)) < 0;
        }

        /// <summary>
        /// Determines if this element has a value equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the values are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean EqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) == value;
        }

        /// <summary>
        /// Determines if this element has a value not equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the values are not equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean NotEqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) != value;
        }

        /// <summary>
        /// Determines if this element has a value greater than to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is greater than the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean GreaterThanValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) > value;
        }

        /// <summary>
        /// Determines if this element has a value greater than or equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is greater than or equal the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean GreaterThanOrEqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) >= value;
        }

        /// <summary>
        /// Determines if this element has a value less than to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is less than the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean LessThanValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) < value;
        }

        /// <summary>
        /// Determines if this element has a value less than or equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is less than or equal to the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean LessThanOrEqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) <= value;
        }

        /// <summary>
        /// Determines if the element has increased it's value by the given value
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value has increased by the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean IncreasedByValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) == unchecked(this.LoadValue(this.PreviousValuePointer) + value);
        }

        /// <summary>
        /// Determines if the element has decreased it's value by the given value
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value has decreased by the given value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean DecreasedByValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) == unchecked(this.LoadValue(this.PreviousValuePointer) - value);
        }

        /// <summary>
        /// Determines if the value is in scientific notation. Only applicable for Single and Double data types.
        /// </summary>
        /// <returns>True if the element is in scientific notation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean IsScientificNotation()
        {
            return this.LoadValue(this.CurrentValuePointer).ToString().ToLower().Contains("e");
        }

        /// <summary>
        /// Gets the current value of this element.
        /// </summary>
        /// <returns>The current value of this element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe DataType GetCurrentValue()
        {
            return this.LoadValue(this.CurrentValuePointer);
        }

        /// <summary>
        /// Gets the previous value of this element.
        /// </summary>
        /// <returns>The previous value of this element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe DataType GetPreviousValue()
        {
            return this.LoadValue(this.PreviousValuePointer);
        }

        /// <summary>
        /// Gets the label of this element.
        /// </summary>
        /// <returns>The label of this element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe LabelType GetElementLabel()
        {
            return this.Parent.GetElementLabels() == null ? default(LabelType) : this.Parent.GetElementLabels()[this.CurrentElementIndex];
        }

        /// <summary>
        /// Sets the label of this element.
        /// </summary>
        /// <param name="newLabel">The new element label.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetElementLabel(LabelType newLabel)
        {
            this.Parent.GetElementLabels()[this.CurrentElementIndex] = newLabel;
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
        /// Loads the value of this snapshot element from the given array.
        /// </summary>
        /// <param name="array">The byte array from which to read a value.</param>
        /// <returns>The value at the start of this array casted as the proper data type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe DataType LoadValue(Byte* array)
        {
            switch (this.CurrentTypeCode)
            {
                case TypeCode.Byte:
                    return (DataType)(object)(*array);
                case TypeCode.SByte:
                    return (DataType)(object)(*(SByte*)array);
                case TypeCode.Int16:
                    return (DataType)(object)(*(Int16*)array);
                case TypeCode.Int32:
                    return (DataType)(object)(*(Int32*)array);
                case TypeCode.Int64:
                    return (DataType)(object)(*(Int64*)array);
                case TypeCode.UInt16:
                    return (DataType)(object)(*(UInt16*)array);
                case TypeCode.UInt32:
                    return (DataType)(object)(*(UInt32*)array);
                case TypeCode.UInt64:
                    return (DataType)(object)(*(UInt64*)array);
                case TypeCode.Single:
                    return (DataType)(object)(*(Single*)array);
                case TypeCode.Double:
                    return (DataType)(object)(*(Double*)array);
                default:
                    throw new Exception("Invalid element type");
            }
        }
    }
    //// End class
}
//// End namespace