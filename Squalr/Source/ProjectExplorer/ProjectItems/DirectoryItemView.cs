namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class DirectoryItemView : ProjectItemView
    {
        public DirectoryItemView(DirectoryItem directoryItem)
        {
            this.DirectoryItem = directoryItem;
        }

        public String FilePath
        {
            get
            {
                return this.DirectoryItem.FilePath;
            }

            set
            {
                this.DirectoryItem.FilePath = value;
            }
        }

        public IEnumerable<ProjectItem> ChildItems
        {
            get
            {
                return this.DirectoryItem.ChildItems;
            }
        }

        [Browsable(false)]
        private DirectoryItem DirectoryItem { get; set; }

        public void AddChild(ProjectItem projectItem)
        {
            this.DirectoryItem.AddChild(projectItem);
        }
    }
    //// End class
}
//// End namespace