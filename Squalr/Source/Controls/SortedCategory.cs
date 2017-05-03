namespace Squalr.Source.Controls
{
    using System;
    using System.ComponentModel;
    using Utils.Extensions;

    public class SortedCategory : CategoryAttribute
    {
        public enum CategoryType
        {
            [Description("Stream")]
            Stream = 0,

            [Description("Common")]
            Common = 1,

            [Description("Advanced")]
            Advanced = 2,
        }

        private const Char NonPrintableChar = '\t';

        public SortedCategory(CategoryType category)
            : base(category.GetDescription().PadLeft(category.GetDescription().Length + Enum.GetNames(typeof(CategoryType)).Length - (Int32)category, SortedCategory.NonPrintableChar))
        {
        }
    }
    //// End class
}
//// End namespace