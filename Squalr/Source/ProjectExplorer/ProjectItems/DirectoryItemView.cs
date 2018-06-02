namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Controls;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class DirectoryItemView : ProjectItemView
    {
        private Boolean isExpanded;

        private DirectoryItem directoryItem;

        public DirectoryItemView(DirectoryItem directoryItem)
        {
            this.DirectoryItem = directoryItem;
        }

        /// <summary>
        /// Gets or sets the description for this object.
        /// </summary>
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Name"), Description("The name of this folder")]
        public String Name
        {
            get
            {
                return this.DirectoryItem.Name;
            }
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

        public override Boolean IsExpanded
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

        public override FullyObservableCollection<ProjectItem> ChildItems
        {
            get
            {
                return this.DirectoryItem.ChildItems;
            }
        }

        [Browsable(false)]
        private DirectoryItem DirectoryItem
        {
            get
            {
                return this.directoryItem;
            }

            set
            {
                this.directoryItem = value;
                this.ProjectItem = value;
                this.RaisePropertyChanged(nameof(this.DirectoryItem));
            }
        }

        public void AddChild(ProjectItem projectItem)
        {
            this.DirectoryItem.AddChild(projectItem);
            this.IsExpanded = true;
            this.RaisePropertyChanged(nameof(this.ChildItems));
        }

        public void RemoveChild(ProjectItem projectItem)
        {
            this.DirectoryItem.RemoveChild(projectItem);
            this.RaisePropertyChanged(nameof(this.ChildItems));
        }
    }
    //// End class
}
//// End namespace