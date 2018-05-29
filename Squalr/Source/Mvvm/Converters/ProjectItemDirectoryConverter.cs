namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Engine.Logging;
    using Squalr.Source.ProjectExplorer;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class ProjectItemDirectoryConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            try
            {
                DirectoryItemView directoryItemView = value as DirectoryItemView;

                if (directoryItemView == null)
                {
                    return null;
                }

                return ProjectItemReader.GetChildFiles(directoryItemView.Path);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error fetching files", ex);
                return null;
            }
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace