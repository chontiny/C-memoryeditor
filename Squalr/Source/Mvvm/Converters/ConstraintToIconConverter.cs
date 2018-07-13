namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Content;
    using Squalr.Engine.Scanning.Scanners.Constraints;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Converts Data Types to a corresponding image.
    /// </summary>
    public class ConstraintToIconConverter : IValueConverter
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
            ScanConstraint.ConstraintType? constraint = null;

            if (value is ScanConstraint)
            {
                constraint = (value as ScanConstraint).Constraint;
            }
            else if (value is ScanConstraint.ConstraintType)
            {
                constraint = (ScanConstraint.ConstraintType)value;
            }

            if (constraint == null)
            {
                return null;
            }

            switch (constraint)
            {
                case ScanConstraint.ConstraintType.Equal:
                    return Images.Equal;
                case ScanConstraint.ConstraintType.NotEqual:
                    return Images.NotEqual;
                case ScanConstraint.ConstraintType.GreaterThan:
                    return Images.GreaterThan;
                case ScanConstraint.ConstraintType.GreaterThanOrEqual:
                    return Images.GreaterThanOrEqual;
                case ScanConstraint.ConstraintType.LessThan:
                    return Images.LessThan;
                case ScanConstraint.ConstraintType.LessThanOrEqual:
                    return Images.LessThanOrEqual;
                case ScanConstraint.ConstraintType.Changed:
                    return Images.Changed;
                case ScanConstraint.ConstraintType.Unchanged:
                    return Images.Unchanged;
                case ScanConstraint.ConstraintType.Increased:
                    return Images.Increased;
                case ScanConstraint.ConstraintType.Decreased:
                    return Images.Decreased;
                case ScanConstraint.ConstraintType.IncreasedByX:
                    return Images.PlusX;
                case ScanConstraint.ConstraintType.DecreasedByX:
                    return Images.MinusX;
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