namespace Squalr.Source.Utils.TypeConverters
{
    using Squalr.Engine.Input.HotKeys;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Hotkey type converter for use in the property viewer.
    /// </summary>
    public class HotkeyConverter : TypeConverter
    {
        /// <summary>
        /// Converts a value to a string indicating whether a hotkey is present.
        /// </summary>
        /// <param name="context">Type descriptor context.</param>
        /// <param name="culture">Globalization info.</param>
        /// <param name="value">The value being converted.</param>
        /// <param name="destinationType">The target type to convert to.</param>
        /// <returns>The converted value.</returns>
        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            if (value == null)
            {
                return "(None)";
            }

            if (value.GetType().GetInterfaces().Any(x => x.IsAssignableFrom(typeof(IEnumerable<Hotkey>))))
            {
                return "(Hotkeys)";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Determines if this converter can convert to the given source type.
        /// </summary>
        /// <param name="context">Type descriptor context.</param>
        /// <param name="sourceType">The source type.</param>
        /// <returns>True if this converter can convert to the given type.</returns>
        public override Boolean CanConvertTo(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        /// <summary>
        /// Determines if this converter can convert from the given source type.
        /// </summary>
        /// <param name="context">Type descriptor context.</param>
        /// <param name="sourceType">The source type.</param>
        /// <returns>True if this converter can convert from the given type.</returns>
        public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }
    }
    //// End class
}
//// End namespace