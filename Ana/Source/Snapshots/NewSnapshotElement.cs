namespace Ana.Source.Snapshots
{
    using System;
    using System.Runtime.CompilerServices;
    using Utils.Extensions;
    internal interface ISnapshotElementRef
    {
        void InitializePointers(Int32 index = 0);

        void IncrementPointers();

        void AddPointers(Int32 alignment);

        void SetValid(Boolean isValid);

        IntPtr GetBaseAddress();

        Boolean Changed();

        Boolean Unchanged();

        Boolean Increased();

        Boolean Decreased();

        Boolean EqualToValue(dynamic value);

        Boolean NotEqualToValue(dynamic value);

        Boolean GreaterThanValue(dynamic value);

        Boolean GreaterThanOrEqualToValue(dynamic value);

        Boolean LessThanValue(dynamic value);

        Boolean LessThanOrEqualToValue(dynamic value);

        Boolean IncreasedByValue(dynamic value);

        Boolean DecreasedByValue(dynamic value);

        Boolean IsScientificNotation();

        Boolean HasCurrentValue();

        Boolean HasPreviousValue();
    }

    internal interface ISnapshotElementRef<DataType, LabelType> : ISnapshotElementRef
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {
        DataType GetCurrentValue();

        DataType GetPreviousValue();

        LabelType GetElementLabel();

        void SetElementLabel(LabelType newLabel);
    }

    internal class SnapshotElementRef<DataType, LabelType> : ISnapshotElementRef<DataType, LabelType>
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
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

        private ISnapshotRegion<DataType, LabelType> Parent { get; set; }

        private unsafe Byte* CurrentValuePointer { get; set; }

        private unsafe Byte* PreviousValuePointer { get; set; }

        private Int32 CurrentElementIndex { get; set; }

        private TypeCode CurrentTypeCode { get; set; }

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

        public void SetValid(Boolean isValid)
        {
            this.Parent.GetValidBits().Set(this.CurrentElementIndex, isValid);
        }

        public IntPtr GetBaseAddress()
        {
            return this.Parent.GetBaseAddress().Add(this.CurrentElementIndex);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe LabelType GetElementLabel()
        {
            return this.Parent.GetElementLabels() == null ? default(LabelType) : this.Parent.GetElementLabels()[this.CurrentElementIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetElementLabel(LabelType newLabel)
        {
            this.Parent.GetElementLabels()[this.CurrentElementIndex] = newLabel;
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
            switch (this.CurrentTypeCode)
            {
                case TypeCode.Byte:
                    return (DataType)(object)*array;
                case TypeCode.SByte:
                    return (DataType)(object)*(SByte*)array;
                case TypeCode.Int16:
                    return (DataType)(object)*(Int16*)array;
                case TypeCode.Int32:
                    return (DataType)(object)*(Int32*)array;
                case TypeCode.Int64:
                    return (DataType)(object)*(Int64*)array;
                case TypeCode.UInt16:
                    return (DataType)(object)*(UInt16*)array;
                case TypeCode.UInt32:
                    return (DataType)(object)*(UInt32*)array;
                case TypeCode.UInt64:
                    return (DataType)(object)*(UInt64*)array;
                case TypeCode.Single:
                    return (DataType)(object)*(Single*)array;
                case TypeCode.Double:
                    return (DataType)(object)*(Double*)array;
                default:
                    throw new Exception("Invalid element type");
            }
        }
    }
    //// End class
}
//// End namespace