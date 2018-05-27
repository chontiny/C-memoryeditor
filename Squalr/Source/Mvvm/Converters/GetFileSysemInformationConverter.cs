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

                //return the subdirectories of the Current Node
                if ((ObjectType)nodeToExpand.DirType == ObjectType.MyComputer)
                {
                    return FileSystemExplorerService.GetRootDirectories().Select(sd => new DirInfo(sd)).ToList();
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

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace