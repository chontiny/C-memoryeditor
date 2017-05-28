namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converter class to convert a project item category to a <see cref="Color"/> value.
    /// </summary>
    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    internal class CategoryToColorConverter : IValueConverter
    {
        private static Brush BuffBrush = new SolidColorBrush(System.Drawing.Color.SteelBlue.ToWpfColor());
        private static Brush GlitchBrush = new SolidColorBrush(System.Drawing.Color.ForestGreen.ToWpfColor());
        private static Brush UtilityBrush = new SolidColorBrush(System.Drawing.Color.DarkOrchid.ToWpfColor());
        private static Brush CurseBrush = new SolidColorBrush(System.Drawing.Color.IndianRed.ToWpfColor());
        private static Brush DefaultBrush = new SolidColorBrush(System.Drawing.Color.Transparent.ToWpfColor());

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
            if (value is ProjectItem.ProjectItemCategory && targetType == typeof(Brush))
            {
                ProjectItem.ProjectItemCategory val = (ProjectItem.ProjectItemCategory)value;

                switch (val)
                {
                    case ProjectItem.ProjectItemCategory.Buff:
                        return CategoryToColorConverter.BuffBrush;
                    case ProjectItem.ProjectItemCategory.Glitch:
                        return CategoryToColorConverter.GlitchBrush;
                    case ProjectItem.ProjectItemCategory.Miscellaneous:
                        return CategoryToColorConverter.UtilityBrush;
                    case ProjectItem.ProjectItemCategory.Curse:
                        return CategoryToColorConverter.CurseBrush;
                    default:
                        return CategoryToColorConverter.DefaultBrush;
                }
            }

            return CategoryToColorConverter.DefaultBrush;
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
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace