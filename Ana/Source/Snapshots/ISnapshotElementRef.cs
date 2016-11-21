namespace Ana.Source.Snapshots
{
    using System;

    /// <summary>
    /// Interface that defines a reference to an element within a snapshot region.
    /// </summary>
    internal partial interface ISnapshotElementRef
    {
        /// <summary>
        /// Initializes snapshot value reference pointers
        /// </summary>
        /// <param name="index">The index of the element to begin pointing to.</param>
        void InitializePointers(Int32 index = 0);

        /// <summary>
        /// Increments all value and label pointers.
        /// </summary>
        void IncrementPointers();

        /// <summary>
        /// Increments all value and label pointers by the given alignment.
        /// </summary>
        /// <param name="alignment">The alignment by which to increment.</param>
        void AddPointers(Int32 alignment);

        /// <summary>
        /// Sets the valid bit of this element.
        /// </summary>
        /// <param name="isValid">Whether or not this element's valid bit is set.</param>
        void SetValid(Boolean isValid);

        /// <summary>
        /// Gets the base address of this element in memory.
        /// </summary>
        /// <returns>The base address of this element in memory.</returns>
        IntPtr GetBaseAddress();

        /// <summary>
        /// Determines if this element has changed.
        /// </summary>
        /// <returns>True if the element changed.</returns>
        Boolean Changed();

        /// <summary>
        /// Determines if this element has not changed.
        /// </summary>
        /// <returns>True if the element is unchanged.</returns>
        Boolean Unchanged();

        /// <summary>
        /// Determines if this element has increased.
        /// </summary>
        /// <returns>True if the element increased.</returns>
        Boolean Increased();

        /// <summary>
        /// Determines if this element has decreased.
        /// </summary>
        /// <returns>True if the element decreased.</returns>
        Boolean Decreased();

        /// <summary>
        /// Determines if this element has a value equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the values are equal.</returns>
        Boolean EqualToValue(dynamic value);

        /// <summary>
        /// Determines if this element has a value not equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the values are not equal.</returns>
        Boolean NotEqualToValue(dynamic value);

        /// <summary>
        /// Determines if this element has a value greater than to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is greater than the given value.</returns>
        Boolean GreaterThanValue(dynamic value);

        /// <summary>
        /// Determines if this element has a value greater than or equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is greater than or equal the given value.</returns>
        Boolean GreaterThanOrEqualToValue(dynamic value);

        /// <summary>
        /// Determines if this element has a value less than to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is less than the given value.</returns>
        Boolean LessThanValue(dynamic value);

        /// <summary>
        /// Determines if this element has a value less than or equal to the given value.
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value is less than or equal to the given value.</returns>
        Boolean LessThanOrEqualToValue(dynamic value);

        /// <summary>
        /// Determines if the element has increased it's value by the given value
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value has increased by the given value.</returns>
        Boolean IncreasedByValue(dynamic value);

        /// <summary>
        /// Determines if the element has decreased it's value by the given value
        /// </summary>
        /// <param name="value">The value being compared against.</param>
        /// <returns>True if the element value has decreased by the given value.</returns>
        Boolean DecreasedByValue(dynamic value);

        /// <summary>
        /// Determines if the value is in scientific notation. Only applicable for Single and Double data types.
        /// </summary>
        /// <returns>True if the element is in scientific notation.</returns>
        Boolean IsScientificNotation();

        /// <summary>
        /// Determines if this element has a current value associated with it.
        /// </summary>
        /// <returns>True if a current value is present.</returns>
        Boolean HasCurrentValue();

        /// <summary>
        /// Determines if this element has a previous value associated with it.
        /// </summary>
        /// <returns>True if a previous value is present.</returns>
        Boolean HasPreviousValue();
    }
    //// End interface

    /// <summary>
    /// Interface that defines a reference to an element within a snapshot region.
    /// </summary>
    /// <typeparam name="DataType">The data type of this snapshot element.</typeparam>
    /// <typeparam name="LabelType">The type corresponding to the labels of this snapshot element.</typeparam>
    internal partial interface ISnapshotElementRef<DataType, LabelType> : ISnapshotElementRef
       where DataType : struct, IComparable<DataType>
       where LabelType : struct, IComparable<LabelType>
    {
        /// <summary>
        /// Gets the current value of this element.
        /// </summary>
        /// <returns>The current value of this element.</returns>
        DataType GetCurrentValue();

        /// <summary>
        /// Gets the previous value of this element.
        /// </summary>
        /// <returns>The previous value of this element.</returns>
        DataType GetPreviousValue();

        /// <summary>
        /// Gets the label of this element.
        /// </summary>
        /// <returns>The label of this element.</returns>
        LabelType GetElementLabel();

        /// <summary>
        /// Sets the label of this element.
        /// </summary>
        /// <param name="newLabel">The new element label.</param>
        void SetElementLabel(LabelType newLabel);
    }
    //// End interface
}
//// End namespace