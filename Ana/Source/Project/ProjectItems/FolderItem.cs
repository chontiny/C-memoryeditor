namespace Ana.Source.Project.ProjectItems
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
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
        /// Removes a project item as a child under this one
        /// </summary>
        /// <param name="projectItem">The child project item</param>
        public void RemoveChild(ProjectItem projectItem)
        {
            projectItem.Parent = this;

            if (this.Children == null)
            {
                this.Children = new List<ProjectItem>();
            }

            if (this.Children.Contains(projectItem))
            {
                this.Children.Remove(projectItem);
            }
        }

        /// <summary>
        /// Adds a project item as a sibling to this one
        /// </summary>
        /// <param name="projectItem">The child project item</param>
        public void AddSibling(ProjectItem projectItem, Boolean after)
        {
            projectItem.Parent = this.Parent;

            if (after)
            {
                this.Parent?.Children?.Insert(this.Parent.Children.IndexOf(this) + 1, projectItem);
            }
            else
            {
                this.Parent?.Children?.Insert(this.Parent.Children.IndexOf(this), projectItem);
            }
        }

        /// <summary>
        /// Deletes the specified children from this item
        /// </summary>
        /// <param name="toDelete">The children to delete</param>
        public void DeleteChildren(IEnumerable<ProjectItem> toDelete)
        {
            if (toDelete == null)
            {
                return;
            }

            // Sort children and nodes to delete (Makes the algorithm O(nlogn) rather than O(n^2))
            IEnumerable<ProjectItem> childrenSorted = this.Children.ToList().OrderBy(x => x.GetHashCode());
            toDelete = toDelete.OrderBy(x => x.GetHashCode());

            if (toDelete.Count() <= 0 || childrenSorted.Count() <= 0)
            {
                return;
            }

            ProjectItem nextDelete = toDelete.First();
            ProjectItem nextNode = childrenSorted.First();

            toDelete = toDelete.Skip(1);
            childrenSorted = childrenSorted.Skip(1);

            // Walk through both lists and see if there are elements in common and delete them
            while (nextDelete != null && nextNode != null)
            {
                if (nextNode.GetHashCode() > nextDelete.GetHashCode())
                {
                    nextDelete = null;
                }
                else if (nextNode.GetHashCode() < nextDelete.GetHashCode())
                {
                    nextNode = null;
                }
                else if (nextNode.GetHashCode() == nextDelete.GetHashCode())
                {
                    this.Children.Remove(nextNode);

                    nextDelete = null;
                    nextNode = null;
                }

                if (nextDelete == null)
                {
                    if (toDelete.Count() <= 0)
                    {
                        break;
                    }

                    nextDelete = toDelete.First();
                    toDelete = toDelete.Skip(1);
                }

                if (nextNode == null)
                {
                    if (childrenSorted.Count() <= 0)
                    {
                        break;
                    }

                    nextNode = childrenSorted.First();
                    childrenSorted = childrenSorted.Skip(1);
                }
            }
        }

        /// <summary>
        /// Reconstructs the parents for all nodes of this graph. Call this from the root
        /// Needed since we cannot serialize this to json or we will get cyclic dependencies
        /// </summary>
        /// <param name="parent"></param>
        public void BuildParents(FolderItem parent = null)
        {
            this.Parent = parent;

            foreach (ProjectItem child in this.Children)
            {
                if (child is FolderItem)
                {
                    (child as FolderItem)?.BuildParents(this);
                }
            }
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
        /// Removes the specified item from this item's children recursively
        /// </summary>
        /// <param name="projectItem">The item to remove</param>
        /// <returns>Returns true if the removal succeeded</returns>
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
    }
    //// End class
}
//// End namespace