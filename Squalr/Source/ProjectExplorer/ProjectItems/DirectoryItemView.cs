namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using Squalr.Engine.Utils.DataStructures;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class DirectoryItemView : ProjectItemView
    {
        private Boolean isExpanded;

        public DirectoryItemView(DirectoryItem directoryItem)
        {
            this.DirectoryItem = directoryItem;
        }

        public String FilePath
        {
            get
            {
                return this.DirectoryItem.DirectoryPath;
            }

            set
            {
                this.DirectoryItem.DirectoryPath = value;
            }
        }

        public Boolean IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                this.isExpanded = value;
                this.RaisePropertyChanged(nameof(this.IsExpanded));
            }
        }

        public FullyObservableCollection<ProjectItem> ChildItems
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
            this.RaisePropertyChanged(nameof(this.ChildItems));
        }
    }
    //// End class
}
//// End namespace