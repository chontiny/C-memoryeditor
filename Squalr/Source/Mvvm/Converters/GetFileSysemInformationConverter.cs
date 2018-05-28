namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Projects;
    using Squalr.Source.SolutionExplorer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    public class GetFileSysemInformationConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            try
            {
                DirInfo nodeToExpand = value as DirInfo;

                if (nodeToExpand == null)
                {
                    return null;
                }

                IList<DirInfo> directories = FileSystemExplorerService.GetChildDirectories(nodeToExpand.Path).Select(dirs => new DirInfo(dirs)).ToList();
                IList<DirInfo> files = FileSystemExplorerService.GetChildFiles(nodeToExpand.Path).Select(dirs => new DirInfo(dirs)).ToList();

                return directories.Concat(files).ToList();
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