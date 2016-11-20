namespace Ana.Source.Snapshots
{
    using System;
    using System.Runtime.CompilerServices;
    internal interface ISnapshotElementRef<DataType, LabelType>
        where DataType : class, IComparable<DataType>
        where LabelType : class, IComparable<LabelType>
    {
        unsafe void InitializePointers(Int32 index = 0);

        unsafe void IncrementPointers();

        unsafe void AddPointers(Int32 alignment);

        unsafe Boolean Changed();

        unsafe Boolean Unchanged();

        unsafe Boolean Increased();

        unsafe Boolean Decreased();

        unsafe Boolean EqualToValue(dynamic value);

        unsafe Boolean NotEqualToValue(dynamic value);

        unsafe Boolean GreaterThanValue(dynamic value);

        unsafe Boolean GreaterThanOrEqualToValue(dynamic value);

        unsafe Boolean LessThanValue(dynamic value);

        unsafe Boolean LessThanOrEqualToValue(dynamic value);

        unsafe Boolean IncreasedByValue(dynamic value);

        unsafe Boolean DecreasedByValue(dynamic value);

        unsafe Boolean IsScientificNotation();

        unsafe DataType GetCurrentValue();

        unsafe DataType GetPreviousValue();

        unsafe Boolean HasCurrentValue();

        unsafe Boolean HasPreviousValue();
    }

    internal class SnapshotElementRef<DataType, LabelType> : ISnapshotElementRef<DataType, LabelType>
        where DataType : class, IComparable<DataType>
        where LabelType : class, IComparable<LabelType>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotElement" /> class
        /// </summary>
        /// <param name="parent">The parent region that contains this element</param>
        public SnapshotElementRef(ISnapshotRegion<DataType, LabelType> parent)
        {
            this.Parent = parent;
        }

        // MISSING:

        // IsValid, BaseAddress, ElementType (explicitly at least)

        protected ISnapshotRegion<DataType, LabelType> Parent { get; set; }

        protected unsafe Byte* CurrentValuePointer { get; set; }

        protected unsafe Byte* PreviousValuePointer { get; set; }

        protected Int32 CurrentElementIndex { get; set; }

        protected TypeCode CurrentTypeCode { get; set; }

        public unsafe void InitializePointers(Int32 index = 0)
        {
            this.CurrentElementIndex = index;
            this.CurrentTypeCode = Type.GetTypeCode(this.Parent.GetType());
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void IncrementPointers()
        {
            this.CurrentElementIndex++;
            this.CurrentValuePointer++;
            this.PreviousValuePointer++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddPointers(Int32 alignment)
        {
            this.CurrentElementIndex += alignment;
            this.CurrentValuePointer += alignment;
            this.PreviousValuePointer += alignment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Changed()
        {
            return !this.LoadValue(this.CurrentValuePointer).Equals(this.LoadValue(this.PreviousValuePointer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Unchanged()
        {
            return this.LoadValue(this.CurrentValuePointer).Equals(this.LoadValue(this.PreviousValuePointer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Increased()
        {
            return this.LoadValue(this.CurrentValuePointer).CompareTo(this.LoadValue(this.PreviousValuePointer)) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean Decreased()
        {
            return this.LoadValue(this.CurrentValuePointer).CompareTo(this.LoadValue(this.PreviousValuePointer)) < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean EqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) == value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean NotEqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) != value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean GreaterThanValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) > value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean GreaterThanOrEqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) >= value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean LessThanValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) < value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean LessThanOrEqualToValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) <= value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean IncreasedByValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) == unchecked(this.LoadValue(this.PreviousValuePointer) + value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean DecreasedByValue(dynamic value)
        {
            return this.LoadValue(this.CurrentValuePointer) == unchecked(this.LoadValue(this.PreviousValuePointer) - value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean IsScientificNotation()
        {
            return (this.LoadValue(this.CurrentValuePointer).ToString()).ToLower().Contains("e");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe DataType GetCurrentValue()
        {
            return this.LoadValue(this.CurrentValuePointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe DataType GetPreviousValue()
        {
            return this.LoadValue(this.PreviousValuePointer);
        }

        public unsafe Boolean HasCurrentValue()
        {
            if (this.CurrentValuePointer == (Byte*)0)
            {
                return false;
            }

            return true;
        }

        public unsafe Boolean HasPreviousValue()
        {
            if (this.PreviousValuePointer == (Byte*)0)
            {
                return false;
            }

            return true;
        }

        private unsafe DataType LoadValue(Byte* array)
        {
            // TODO: Inline IL could be an optimization here, since *(DataType*)array is not allowed
            switch (this.CurrentTypeCode)
            {
                case TypeCode.Byte:
                    return *array as DataType;
                case TypeCode.SByte:
                    return *(SByte*)array as DataType;
                case TypeCode.Int16:
                    return *(Int16*)array as DataType;
                case TypeCode.Int32:
                    return *(Int32*)array as DataType;
                case TypeCode.Int64:
                    return *(Int64*)array as DataType;
                case TypeCode.UInt16:
                    return *(UInt16*)array as DataType;
                case TypeCode.UInt32:
                    return *(UInt32*)array as DataType;
                case TypeCode.UInt64:
                    return *(UInt64*)array as DataType;
                case TypeCode.Single:
                    return *(Single*)array as DataType;
                case TypeCode.Double:
                    return *(Double*)array as DataType;
                default:
                    throw new Exception("Invalid element type");
            }
        }
    }
    //// End class
}
//// End namespace