namespace Squalr.Source.Mvvm.Converters
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Projects;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    public class ProjectItemViewConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (value as ProjectItem != null)
            {
                return this.ConvertToProjectItemView(value as ProjectItem);
            }

            if (value as IEnumerable<ProjectItem> != null)
            {
                return (value as IEnumerable<ProjectItem>).Select(projectItem => this.ConvertToProjectItemView(projectItem)).Where(projectItem => projectItem != null);
            }

            return null;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ProjectItemView ConvertToProjectItemView(ProjectItem projectItem)
        {
            switch (projectItem)
            {
                case DirectoryItem _ when projectItem is DirectoryItem:
                    return new DirectoryItemView(projectItem as DirectoryItem);
                case ProjectItem _ when projectItem is PointerItem:
                    return new PointerItemView(projectItem as PointerItem);
                case ProjectItem _ when projectItem is ScriptItem:
                    return new ScriptItemView(projectItem as ScriptItem);
                case ProjectItem _ when projectItem is InstructionItem:
                    return new InstructionItemView(projectItem as InstructionItem);
                case ProjectItem _ when projectItem is DotNetItem:
                    return new DotNetItemView(projectItem as DotNetItem);
                case ProjectItem _ when projectItem is JavaItem:
                    return new JavaItemView(projectItem as JavaItem);
                default:
                    Logger.Log(LogLevel.Error, "Unknown project item type");
                    return null;
            }
        }
    }
    //// End class
}
//// End namespace