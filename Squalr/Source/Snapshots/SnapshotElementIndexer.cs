namespace Squalr.Source.Snapshots
{
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines a reference to an element within a snapshot region.
    /// </summary>
    internal unsafe class SnapshotElementIndexer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotElementIndexer" /> class.
        /// </summary>
        /// <param name="parent">The parent region that contains this element.</param>
        /// <param name="elementIndex">The index of the element to begin pointing to.</param>
        /// <param name="compareActionConstraint">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        /// <param name="pointerIncrementMode">The method by which to increment element pointers.</param>
        public unsafe SnapshotElementIndexer(
            SnapshotRegion parent,
            UInt32 elementIndex = 0)
        {
            this.Parent = parent;
            this.ElementIndex = elementIndex;
        }

        /// <summary>
        /// Gets the base address of this element.
        /// </summary>
        public IntPtr BaseAddress
        {
            get
            {
                return this.Parent.ReadGroup.BaseAddress.Add(this.Parent.ReadGroupOffset).Add(this.ElementIndex * this.Parent.ReadGroup.Alignment);
            }
        }

        /// <summary>
        /// Gets or sets the label associated with this element.
        /// </summary>
        public Object ElementLabel
        {
            get
            {
                return this.Parent.ReadGroup.ElementLabels[this.ElementIndex];
            }

            set
            {
                this.Parent.ReadGroup.ElementLabels[this.ElementIndex] = value;
            }
        }

        /// <summary>
        /// Gets or sets the parent snapshot region.
        /// </summary>
        private SnapshotRegion Parent { get; set; }

        /// <summary>
        /// Gets the index of this element.
        /// </summary>
        private unsafe UInt32 ElementIndex { get; set; }

        /// <summary>
        /// Gets or sets the data type of this element.
        /// </summary>
        private DataType DataType { get; set; }

        public Object LoadCurrentValue()
        {
            fixed (Byte* pointerBase = &this.Parent.ReadGroup.CurrentValues[this.Parent.ReadGroupOffset + this.ElementIndex])
            {
                switch (this.DataType)
                {
                    case DataType type when type == DataTypes.Byte:
                        return *pointerBase;
                    case DataType type when type == DataTypes.SByte:
                        return *(SByte*)pointerBase;
                    case DataType type when type == DataTypes.Int16:
                        return *(Int16*)pointerBase;
                    case DataType type when type == DataTypes.Int32:
                        return *(Int32*)pointerBase;
                    case DataType type when type == DataTypes.Int64:
                        return *(Int64*)pointerBase;
                    case DataType type when type == DataTypes.UInt16:
                        return *(UInt16*)pointerBase;
                    case DataType type when type == DataTypes.UInt32:
                        return *(UInt32*)pointerBase;
                    case DataType type when type == DataTypes.UInt64:
                        return *(UInt64*)pointerBase;
                    case DataType type when type == DataTypes.Single:
                        return *(Single*)pointerBase;
                    case DataType type when type == DataTypes.Double:
                        return *(Double*)pointerBase;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public Object LoadPreviousValue()
        {
            fixed (Byte* pointerBase = &this.Parent.ReadGroup.PreviousValues[this.Parent.ReadGroupOffset + this.ElementIndex])
            {
                switch (this.DataType)
                {
                    case DataType type when type == DataTypes.Byte:
                        return *pointerBase;
                    case DataType type when type == DataTypes.SByte:
                        return *(SByte*)pointerBase;
                    case DataType type when type == DataTypes.Int16:
                        return *(Int16*)pointerBase;
                    case DataType type when type == DataTypes.Int32:
                        return *(Int32*)pointerBase;
                    case DataType type when type == DataTypes.Int64:
                        return *(Int64*)pointerBase;
                    case DataType type when type == DataTypes.UInt16:
                        return *(UInt16*)pointerBase;
                    case DataType type when type == DataTypes.UInt32:
                        return *(UInt32*)pointerBase;
                    case DataType type when type == DataTypes.UInt64:
                        return *(UInt64*)pointerBase;
                    case DataType type when type == DataTypes.Single:
                        return *(Single*)pointerBase;
                    case DataType type when type == DataTypes.Double:
                        return *(Double*)pointerBase;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        /// <summary>
        /// Gets the label of this element.
        /// </summary>
        /// <returns>The label of this element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Object GetElementLabel()
        {
            return this.Parent.ReadGroup.ElementLabels == null ? null : this.Parent.ReadGroup.ElementLabels[this.ElementIndex];
        }

        /// <summary>
        /// Sets the label of this element.
        /// </summary>
        /// <param name="newLabel">The new element label.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetElementLabel(Object newLabel)
        {
            this.Parent.ReadGroup.ElementLabels[this.ElementIndex] = newLabel;
        }

        /// <summary>
        /// Determines if this element has a current value associated with it.
        /// </summary>
        /// <returns>True if a current value is present.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Boolean HasCurrentValue()
        {
            if (this.Parent.ReadGroup.CurrentValues.IsNullOrEmpty())
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
            if (this.Parent.ReadGroup.PreviousValues.IsNullOrEmpty())
            {
                return false;
            }

            return true;
        }
    }
    //// End class
}
//// End namespace