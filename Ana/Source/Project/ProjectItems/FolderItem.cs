namespace Ana.Source.Project.ProjectItems
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// A folder project item which may contain other project items
    /// </summary>
    [DataContract]
    internal class FolderItem : ProjectItem
    {

        [Browsable(false)]
        private List<ProjectItem> children;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderItem" /> class
        /// </summary>
        public FolderItem() : this("New Folder")
        {
            this.children = new List<ProjectItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderItem" /> class
        /// </summary>
        /// <param name="description">The description of the folder</param>
        public FolderItem(String description) : base(description)
        {
        }

        /// <summary>
        /// Gets or sets the children of this project item
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public List<ProjectItem> Children
        {
            get
            {
                if (this.children == null)
                {
                    this.children = new List<ProjectItem>();
                }

                return this.children;
            }

            set
            {
                this.children = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Children));
            }
        }

        /// <summary>
        /// Clones the project item.
        /// </summary>
        /// <returns>The clone of the project.</returns>
        public override ProjectItem Clone()
        {
            FolderItem clone = new FolderItem();
            clone.description = this.Description;
            clone.parent = this.Parent;
            clone.children = new List<ProjectItem>();

            if (this.Children != null && this.Children.Count > 0)
            {
                foreach (ProjectItem projectItem in this.Children)
                {
                    clone.children.Add(projectItem.Clone());
                }
            }

            return clone;
        }

        public override void Update()
        {
            this.Children.ForEach(x => x.Update());
        }

        /// <summary>
        /// Adds a project item as a child under this one
        /// </summary>
        /// <param name="projectItem">The child project item</param>
        public void AddChild(ProjectItem projectItem)
        {
            projectItem.Parent = this;

            if (this.Children == null)
            {
                this.Children = new List<ProjectItem>();
            }

            this.Children.Add(projectItem);
        }

        /// <summary>
        /// Determines if this item or any of its children contain an item
        /// </summary>
        /// <param name="projectItem">The item to search for</param>
        /// <returns>Returns true if the item is found</returns>
        public Boolean HasNode(ProjectItem projectItem)
        {
            if (this.Children.Contains(projectItem))
            {
                return true;
            }

            foreach (ProjectItem child in this.Children)
            {
                if (child is FolderItem)
                {
                    if (child != null && (child as FolderItem).HasNode(projectItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the specified items from this item's children recursively.
        /// </summary>
        /// <param name="projectItem">The items to remove.</param>
        public void RemoveNodes(IEnumerable<ProjectItem> projectItems)
        {
            if (projectItems == null)
            {
                return;
            }

            foreach (ProjectItem projectItem in projectItems)
            {
                this.RemoveNode(projectItem);
            }
        }

        /// <summary>
        /// Removes the specified item from this item's children recursively.
        /// </summary>
        /// <param name="projectItem">The item to remove.</param>
        /// <returns>Returns true if the removal succeeded.</returns>
        public Boolean RemoveNode(ProjectItem projectItem)
        {
            if (projectItem == null)
            {
                return false;
            }

            if (this.Children.Contains(projectItem))
            {
                projectItem.Parent = null;
                this.Children.Remove(projectItem);
                return true;
            }
            else
            {
                foreach (ProjectItem child in this.Children)
                {
                    if (child is FolderItem)
                    {
                        if (child != null && (child as FolderItem).RemoveNode(projectItem))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected override Boolean IsActivatable()
        {
            return false;
        }
    }
    //// End class
}
//// End namespace