namespace Squalr.Source.Mvvm.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converter class to convert a boolean to a <see cref="Visibility"/> value.
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToVisibilityConverter" /> class.
        /// </summary>
        public BooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed)
        {
        }
    }
    //// End class
}
//// End namespace