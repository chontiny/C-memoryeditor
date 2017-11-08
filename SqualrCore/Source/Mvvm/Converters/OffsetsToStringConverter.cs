namespace SqualrCore.Source.Mvvm.Converters
{
    using SqualrCore.Source.Utils;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Converts a list of integers value to a string value.
    /// </summary>
    public class OffsetsToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts an Int32 to a Hex string.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">Globalization info.</param>
        /// <returns>A hex string. If conversion cannot take place, returns null.</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return String.Empty;
            }

            if (value is IEnumerable<Int32>)
            {
                String result = String.Empty;

                foreach (Int32 offset in (value as IEnumerable<Int32>))
                {
                    if (offset < 0)
                    {
                        result += "-" + Conversions.ToHex<Int32>(-offset, formatAsAddress: false) + ", ";
                    }
                    else
                    {
                        result += Conversions.ToHex<Int32>(offset, formatAsAddress: false) + ", ";
                    }
                }

                return result.TrimEnd(',', ' ');
            }

            return String.Empty;
        }

        /// <summary>
        /// Hex string to an Int32.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">Globalization info.</param>
        /// <returns>An Int32. If conversion cannot take place, returns 0.</returns>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace