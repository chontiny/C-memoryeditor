namespace Ana.Source.Mvvm.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    internal class IndexConverter : IValueConverter
    {
        public Object Convert(Object value, Type TargetType, Object parameter, CultureInfo culture)
        {
            ListViewItem item = value as ListViewItem;
            ItemsControl listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            Int32 index = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;

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