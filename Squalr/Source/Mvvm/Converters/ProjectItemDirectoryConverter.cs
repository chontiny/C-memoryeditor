namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Engine.Projects;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    public class ProjectItemDirectoryConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            DirectoryItemView directoryItem = value as DirectoryItemView;

            if (directoryItem == null)
            {
                return null;
            }

            IList<ProjectItemView> projectItems = new List<ProjectItemView>();

            foreach (ProjectItem projectItem in directoryItem.ChildItems)
            {
                ProjectItemView projectItemView = (new ProjectItemViewConverter()).Convert(projectItem, typeof(ProjectItemView), parameter, culture) as ProjectItemView;

                if (projectItemView != null)
                {
                    projectItems.Add(projectItemView);
                }
            }

            return projectItems;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace