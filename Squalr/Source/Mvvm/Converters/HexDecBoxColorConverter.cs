namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Source.Controls;
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converts a HexDec box to a display color.
    /// </summary>
    public class HexDecBoxColorConverter : IValueConverter
    {
        private static readonly Brush DecBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private static readonly Brush HexBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x22, 0x8B, 0x22));

        private static readonly Brush InvalidBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0, 0));

        /// <summary>
        /// Converts a HexDec box to a display color.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">Globalization info.</param>
        /// <returns>The display color for the HexDec box.</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            HexDecBoxViewModel hexDecBoxViewModel = value as HexDecBoxViewModel;

            if (hexDecBoxViewModel != null)
            {
                if (!hexDecBoxViewModel.IsValid)
                {
                    return HexDecBoxColorConverter.InvalidBrush;
                }

                if (hexDecBoxViewModel.IsHex)
                {
                    return HexDecBoxColorConverter.HexBrush;
                }
            }

            return HexDecBoxColorConverter.DecBrush;
        }

        /// <summary>
        /// Not used or implemented.
        /// </summary>
        /// <param name="value">Value to be converted.</param>
        /// <param name="targetType">Type to convert to.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="culture">Globalization info.</param>
        /// <returns>Throws see <see cref="NotImplementedException" />.</returns>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace