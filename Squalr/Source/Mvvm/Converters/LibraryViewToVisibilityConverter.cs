namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Source.Browse.Library;
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converter class to convert a boolean to a <see cref="Visibility"/> value.
    /// </summary>
    [ValueConversion(typeof(LibraryView), typeof(Visibility))]
    internal class LibraryViewToVisibilityConverter : IValueConverter
    {
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
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is LibraryView && targetType == typeof(Visibility))
            {
                LibraryView val = (LibraryView)value;
                LibraryView targetVal = (LibraryView)parameter;

                if (val == targetVal)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
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
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    //// End class
}
//// End namespace