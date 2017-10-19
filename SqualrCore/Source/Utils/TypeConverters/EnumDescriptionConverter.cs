namespace SqualrCore.Source.Utils.TypeConverters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    internal class EnumDescriptionConverter : EnumConverter
    {
        private Type enumType;

        public EnumDescriptionConverter(Type type) : base(type)
        {
            enumType = type;
        }

        public override Boolean CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(String);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destType)
        {
            FieldInfo fieldInfo = enumType.GetField(Enum.GetName(enumType, value));
            DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

            if (dna != null)
            {
                return dna.Description;
            }
            else
            {
                return value.ToString();
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(String);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
        {
            foreach (FieldInfo fieldInfo in enumType.GetFields())
            {
                DescriptionAttribute dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

                if ((dna != null) && ((String)value == dna.Description))
                {
                    return Enum.Parse(enumType, fieldInfo.Name);
                }
            }
            return Enum.Parse(enumType, (String)value);
        }
    }
}
