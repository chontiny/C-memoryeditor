namespace SqualrCore.Source.Mvvm.Converters
{
    using SqualrCore.Source.ProjectItems;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Attempts to convert a display value for a given project item.
    /// </summary>
    public class ProjectItemValueConverter : IValueConverter
    {
        /// <summary>
        /// Converts an Icon to a BitmapSource.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">Globalization info.</param>
        /// <returns>Object with type of BitmapSource. If conversion cannot take place, returns null.</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            switch (value.GetType())
            {
                case Type type when typeof(AddressItem).IsAssignableFrom(type):
                    return (value as AddressItem).AddressValue?.ToString();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Not used or implemented.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">Globalization info.</param>
        /// <returns>Throws see <see cref="NotImplementedException" /></returns>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace