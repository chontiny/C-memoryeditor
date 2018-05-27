namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Source.SolutionExplorer;
    using System;
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

                // return the subdirectories of the Current Node
                if ((ObjectType)nodeToExpand.DirType == ObjectType.MyComputer)
                {
                    return FileSystemExplorerService.GetRootDirectories().Select(rootDirectory => new DirInfo(rootDirectory)).ToList();
                }
                else
                {
                    return FileSystemExplorerService.GetChildDirectories(nodeToExpand.Path).Select(dirs => new DirInfo(dirs)).ToList();
                }
            }
            catch
            {
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