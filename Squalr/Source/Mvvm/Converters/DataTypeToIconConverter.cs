namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Content;
    using Squalr.Engine.DataTypes;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Converts DataTypes to an icon format readily usable by the view.
    /// </summary>
    public class DataTypeToIconConverter : IValueConverter
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
            if (parameter != null)
            {
                value = parameter;
            }

            if (value is Type)
            {
                value = new DataType(value as Type);
            }

            switch (value)
            {
                case DataType type when type == DataType.Byte:
                    return Images.PurpleBlocks1;
                case DataType type when type == DataType.Char:
                    return Images.PurpleBlocks1;
                case DataType type when type == DataType.SByte:
                    return Images.BlueBlocks1;
                case DataType type when type == DataType.Int16:
                    return Images.BlueBlocks2;
                case DataType type when type == DataType.Int32:
                    return Images.BlueBlocks4;
                case DataType type when type == DataType.Int64:
                    return Images.BlueBlocks4;
                case DataType type when type == DataType.UInt16:
                    return Images.PurpleBlocks2;
                case DataType type when type == DataType.UInt32:
                    return Images.PurpleBlocks4;
                case DataType type when type == DataType.UInt64:
                    return Images.PurpleBlocks8;
                case DataType type when type == DataType.Single:
                    return Images.OrangeBlocks4;
                case DataType type when type == DataType.Double:
                    return Images.OrangeBlocks8;
                case DataType type when type == DataType.IntPtr:
                    return !Environment.Is64BitProcess ? Images.BlueBlocks4 : Images.BlueBlocks8;
                case DataType type when type == DataType.UIntPtr:
                    return !Environment.Is64BitProcess ? Images.PurpleBlocks4 : Images.PurpleBlocks8;
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
        /// <returns>Throws see <see cref="NotImplementedException" />.</returns>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace