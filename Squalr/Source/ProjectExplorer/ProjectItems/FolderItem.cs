namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Controls;
    using Squalr.Source.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using Utils.TypeConverters;

    /// <summary>
    /// Defines a folder that can be added to the project explorer, which can contain other project items.
    /// </summary>
    [DataContract]
    internal class FolderItem : ProjectItem
    {
        /// <summary>
        /// The children of this folder item.
        /// </summary>
        [Browsable(false)]
        private List<ProjectItem> children;

        /// <summary>
        /// The type of this folder.
        /// </summary>
        [Browsable(false)]
        private FolderTypeEnum folderType;

        /// <summary>
        /// A value indicating if this folder is exported as a single group.
        /// </summary>
        [Browsable(false)]
        private Boolean exportStop;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderItem" /> class.
        /// </summary>
        public FolderItem() : this("New Folder")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderItem" /> class.
        /// </summary>
        /// <param name="description">The description of the folder.</param>
        public FolderItem(String description) : base(description)
        {
            this.children = new List<ProjectItem>();
            this.ChildrenLock = new Object();
        }

        /// <summary>
        /// Defines the activation behavior of a folder.
        /// </summary>
        public enum FolderTypeEnum
        {
            [Description("Normal")]
            Normal,

            [Description("Group")]
            Group,

            [Description("Unique Group")]
            UniqueGroup,
        }

        /// <summary>
        /// Gets or sets the identifier type for this address item.
        /// </summary>
        [DataMember]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(EnumDescriptionConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Folder Type"), Description("Defines the behavior for activating this folder and the items contained by this folder")]
        public FolderTypeEnum FolderType
        {
            get
            {
                return this.folderType;
            }

            set
            {
                if (this.folderType == value)
                {
                    return;
                }

                this.folderType = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.FolderType));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if this folder is exported as a single group.
        /// </summary>
        [DataMember]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Export Stop"), Description("Indicates if this folder is exported as a group")]
        public Boolean ExportStop
        {
            get
            {
                return this.exportStop;
            }

            set
            {
                if (this.exportStop == value)
                {
                    return;
                }

                this.exportStop = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.ExportStop));
                ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the children of this project item.
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
        /// Gets or sets a lock for the access of the children in this folder.
        /// </summary>
        private Object ChildrenLock { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public new void OnDeserialized(StreamingContext streamingContext)
        {
            this.ChildrenLock = new Object();

            if (this.Children == null)
            {
                this.children = new List<ProjectItem>();
            }
        }

        /// <summary>
        /// Clones the project item.
        /// </summary>
        /// <returns>The clone of the project item.</returns>
        public override ProjectItem Clone()
        {
            FolderItem clone = new FolderItem();
            clone.description = this.Description;
            clone.extendedDescription = this.ExtendedDescription;
            clone.folderType = this.FolderType;
            clone.exportStop = this.exportStop;
            clone.parent = this.Parent;
            clone.children = new List<ProjectItem>();

            lock (this.ChildrenLock)
            {
                if (this.Children != null && this.Children.Count > 0)
                {
                    foreach (ProjectItem projectItem in this.Children)
                    {
                        clone.AddChild(projectItem.Clone());
                    }
                }
            }

            return clone;
        }

        /// <summary>
        /// Returns all items under this folder flattened as a single list.
        /// </summary>
        /// <returns>All items under this folder.</returns>
        public List<ProjectItem> Flatten(Func<ProjectItem, Boolean> recursionPredicate = null)
        {
            List<ProjectItem> flattenedItems = new List<ProjectItem>();
            recursionPredicate = recursionPredicate ?? ((projectItem) => projectItem is FolderItem);

            this.FlattenHelper(flattenedItems, recursionPredicate);

            return flattenedItems;
        }

        /// <summary>
        /// Recursively finds a project item by the given guid.
        /// </summary>
        /// <param name="guid">The project item guid.</param>
        /// <returns>The project item if found, otherwise null.</returns>
        public ProjectItem FindProjectItemByGuid(Guid guid)
        {
            foreach (ProjectItem projectItem in this.Flatten())
            {
                if (projectItem.Guid == guid)
                {
                    return projectItem;
                }
            }

            return null;
        }

        /// <summary>
        /// Update event for this project item. Updates all children.
        /// </summary>
        public override void Update()
        {
            lock (this.ChildrenLock)
            {
                this.Children.ForEach(x => x.Update());
            }
        }

        /// <summary>
        /// Reconstructs the parents for all nodes of this graph. Call this from the root.
        /// Needed since we cannot serialize the parent to json or we will get cyclic dependencies.
        /// </summary>
        /// <param name="parent">The parent of this project item.</param>
        public override void BuildParents(FolderItem parent = null)
        {
            base.BuildParents(parent);

            lock (this.ChildrenLock)
            {
                foreach (ProjectItem child in this.Children)
                {
                    child.BuildParents(this as FolderItem);
                }
            }
        }

        /// <summary>
        /// Adds a project item as a child under this one.
        /// </summary>
        /// <param name="newChild">The child project item.</param>
        public void AddChild(ProjectItem newChild)
        {
            lock (this.ChildrenLock)
            {
                newChild.Parent = this;

                if (this.Children == null)
                {
                    this.Children = new List<ProjectItem>();
                }

                this.Children.Add(newChild);
            }

            if (ProjectExplorerViewModel.GetInstance().ProjectRoot.HasNode(this) && ProjectExplorerViewModel.GetInstance().ProjectRoot.HasNode(newChild))
            {
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Adds a project item as a sibling to the specified object.
        /// </summary>
        /// <param name="targetChild">The child project item.</param>
        /// <param name="newChild">The new child project item to add as a sibling.</param>
        /// <param name="after">A value indicating whether or not the new child should be inserted before or after the target.</param>
        public void AddSibling(ProjectItem targetChild, ProjectItem newChild, Boolean after)
        {
            lock (this.ChildrenLock)
            {
                if (!this.Children.Contains(targetChild))
                {
                    return;
                }

                newChild.Parent = this;

                if (after)
                {
                    this.Children?.Insert(this.Children.IndexOf(targetChild) + 1, newChild);
                }
                else
                {
                    this.Children?.Insert(this.Children.IndexOf(targetChild), newChild);
                }
            }

            ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
        }

        /// <summary>
        /// Determines if this item or any of its children contain an item.
        /// </summary>
        /// <param name="projectItem">The item to search for.</param>
        /// <returns>Returns true if the item is found.</returns>
        public Boolean HasNode(ProjectItem projectItem)
        {
            lock (this.ChildrenLock)
            {
                if (this == projectItem || this.Children.Contains(projectItem))
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
            }

            return false;
        }

        /// <summary>
        /// Removes the specified items from this item's children recursively.
        /// </summary>
        /// <param name="projectItems">The items to remove.</param>
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
        /// Removes the specified items from this item's children recursively.
        /// </summary>
        /// <param name="projectItems">The items to remove.</param>
        public void RemoveAllNodes()
        {
            lock (this.ChildrenLock)
            {
                foreach (ProjectItem projectItem in this.Children.ToArray())
                {
                    this.RemoveNode(projectItem);
                }
            }
        }

        /// <summary>
        /// Removes the specified item from this item's children recursively.
        /// </summary>
        /// <param name="projectItem">The item to remove.</param>
        /// <param name="dispose">A value indicating whether the resources of the project item should be disposed.</param>
        /// <returns>Returns true if the removal succeeded.</returns>
        public Boolean RemoveNode(ProjectItem projectItem, Boolean dispose = true)
        {
            Boolean removeSuccess = false;

            if (projectItem == null)
            {
                return false;
            }

            lock (this.ChildrenLock)
            {
                if (this.Children.Contains(projectItem))
                {
                    projectItem.Parent = null;
                    this.Children.Remove(projectItem);
                    removeSuccess = true;

                    if (dispose)
                    {
                        projectItem.Dispose();
                        (projectItem as FolderItem)?.RemoveAllNodes();
                    }
                }
                else
                {
                    foreach (ProjectItem child in this.Children)
                    {
                        if (child is FolderItem)
                        {
                            if ((child as FolderItem).RemoveNode(projectItem, dispose))
                            {
                                removeSuccess = true;
                            }
                        }
                    }
                }
            }

            ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;

            return removeSuccess;
        }

        /// <summary>
        /// Associates a cheat with this project item.
        /// </summary>
        /// <param name="cheat">The associated cheat</param>
        public override void AssociateCheat(Cheat cheat)
        {
            base.AssociateCheat(cheat);

            if (this.Children != null)
            {
                foreach (ProjectItem projectItem in this.Children)
                {
                    projectItem.AssociateCheat(cheat);
                }
            }
        }

        /// <summary>
        /// Function indicating if this script can be activated.
        /// </summary>
        /// <returns>Always false for folders.</returns>
        protected override Boolean IsActivatable()
        {
            switch (this.FolderType)
            {
                case FolderTypeEnum.Group:
                    return true;
                case FolderTypeEnum.UniqueGroup:
                case FolderTypeEnum.Normal:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Helper function for flattening all project items under this folder.
        /// </summary>
        /// <param name="flattenedList">The current list of flattened items.</param>
        private void FlattenHelper(List<ProjectItem> flattenedList, Func<ProjectItem, Boolean> recursionPredicate)
        {
            flattenedList.Add(this);

            foreach (ProjectItem projectItem in this.Children)
            {
                if (recursionPredicate(projectItem))
                {
                    FolderItem folderItem = projectItem as FolderItem;
                    folderItem.FlattenHelper(flattenedList, recursionPredicate);
                }
                else
                {
                    flattenedList.Add(projectItem);
                }
            }
        }
    }
    //// End class
}
//// End namespace