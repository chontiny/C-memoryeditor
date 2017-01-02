namespace Ana.Source.Mvvm.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Data;
    using Utils.Extensions;

    internal class DataGridIndexConverter : IValueConverter
    {
        public Object Convert(Object value, Type TargetType, Object parameter, CultureInfo culture)
        {
            DataGridCell item = value as DataGridCell;
            DataGridRow dataGridRow = item?.Ancestors().OfType<DataGridRow>().FirstOrDefault();
            DataGrid dataGrid = dataGridRow?.Ancestors().OfType<DataGrid>().FirstOrDefault();
            Int32 index = dataGrid?.ItemContainerGenerator.IndexFromContainer(dataGridRow) ?? -1;

            return index.ToString();
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace