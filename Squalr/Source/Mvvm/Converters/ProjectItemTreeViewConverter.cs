namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Projects;
    using Squalr.Source.ProjectExplorer;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class ProjectItemTreeViewConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            try
            {
                ProjectItem nodeToExpand = value as ProjectItem;

                if (nodeToExpand == null)
                {
                    return null;
                }

                return ProjectItemReader.GetChildFiles(nodeToExpand.Path);
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