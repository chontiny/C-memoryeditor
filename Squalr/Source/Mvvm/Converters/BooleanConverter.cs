namespace Squalr.Source.Mvvm.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Converter class to convert a boolean to an arbitrary value.
    /// </summary>
    /// <typeparam name="T">The target conversion type.</typeparam>
    public class BooleanConverter<T> : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanConverter{T}" /> class.
        /// </summary>
        /// <param name="trueValue">The value when the converter is true.</param>
        /// <param name="falseValue">The value when the converter is false.</param>
        public BooleanConverter(T trueValue, T falseValue)
        {
            this.True = trueValue;
            this.False = falseValue;
        }

        /// <summary>
        /// Gets or sets the conversion value for a true value.
        /// </summary>
        public T True { get; set; }

        /// <summary>
        /// Gets or sets the conversion value for a false value.
        /// </summary>
        public T False { get; set; }

        /// <summary> 
        /// Converts a value. 
        /// </summary> 
        /// <param name="value">The value produced by the binding source.</param> 
        /// <param name="targetType">The type of the binding target property.</param> 
        /// <param name="parameter">The converter parameter to use.</param> 
        /// <param name="culture">The culture to use in the converter.</param> 
        /// <returns> 
        /// A converted value. If the method returns null, the valid null value is used. 
        /// </returns> 
        public virtual Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return value is Boolean && ((Boolean)value) ? this.True : this.False;
        }

        /// <summary> 
        /// Converts a value. 
        /// </summary> 
        /// <param name="value">The value that is produced by the binding target.</param> 
        /// <param name="targetType">The type to convert to.</param> 
        /// <param name="parameter">The converter parameter to use.</param> 
        /// <param name="culture">The culture to use in the converter.</param> 
        /// <returns> 
        /// A converted value. If the method returns null, the valid null value is used. 
        /// </returns> 
        public virtual Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, this.True);
        }
    }
    //// End class
}
//// End namespace