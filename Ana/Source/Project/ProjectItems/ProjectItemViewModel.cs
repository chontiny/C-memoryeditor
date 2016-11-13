namespace Ana.Source.Project.ProjectItems
{
    using Content;
    using Controls;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using Utils.Extensions;

    internal class ProjectItemViewModel : TreeViewItemViewModel
    {
        private readonly ProjectItem projectItem;
        private ObservableCollection<TreeViewItemViewModel> children;

        public ProjectItemViewModel(ProjectItem projectItem, TreeViewItemViewModel parent = null) : base(parent)
        {
            this.projectItem = projectItem;
            this.children = new ObservableCollection<TreeViewItemViewModel>();
        }

        public override ObservableCollection<TreeViewItemViewModel> Children
        {
            get
            {
                return this.children;
            }
        }

        public ProjectItem ProjectItem
        {
            get
            {
                return this.projectItem;
            }
        }

        public BitmapImage Icon
        {
            get
            {
                if (this.ProjectItem is FolderItem)
                {
                    return Images.Open;
                }
                else if (this.ProjectItem is AddressItem)
                {
                    return Images.CollectValues;
                }
                else if (this.ProjectItem is ScriptItem)
                {
                    return Images.CollectValues;
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddChild(ProjectItemViewModel child)
        {
            if (!(this.ProjectItem is FolderItem))
            {
                return;
            }

            (this.ProjectItem as FolderItem).Parent = (this.projectItem as FolderItem);

            // Do addition for both view model and underlying project items
            (this.ProjectItem as FolderItem).AddChild(child.ProjectItem);
            this.children = new ObservableCollection<TreeViewItemViewModel>(this.Children.Select(x => x).Append(child).ToList());

            this.RaisePropertyChanged(nameof(this.Children));
        }

        public void InsertChild(ProjectItemViewModel child, Int32 index)
        {
            if (!(this.ProjectItem is FolderItem))
            {
                return;
            }

            (this.ProjectItem as FolderItem).Parent = (this.projectItem as FolderItem);

            // Do insertion for both view model and underlying project items
            (this.ProjectItem as FolderItem).Children.Insert(index, child.projectItem);
            IList<TreeViewItemViewModel> currentChildren = this.Children.ToList();
            currentChildren.Insert(index, child);
            this.children = new ObservableCollection<TreeViewItemViewModel>(currentChildren);

            this.RaisePropertyChanged(nameof(this.Children));
        }

        public void RemoveChildImmediate(ProjectItemViewModel child)
        {
            if (!(this.ProjectItem is FolderItem))
            {
                return;
            }

            // Do removal for both view model and underlying project items
            (this.ProjectItem as FolderItem).RemoveChild(child.ProjectItem);
            this.children = new ObservableCollection<TreeViewItemViewModel>(this.Children.Select(x => x).Where(x => x != child).ToList());
            this.RaisePropertyChanged(nameof(this.Children));
        }

        public void RemoveChildRecursive(ProjectItemViewModel removeTarget)
        {
            if (!(this.ProjectItem is FolderItem))
            {
                return;
            }

            if (this.Children.Contains(removeTarget))
            {
                this.RemoveChildImmediate(removeTarget);
            }
            else
            {
                this.Children.ForEach(x => (x as ProjectItemViewModel).RemoveChildRecursive(removeTarget));
            }
        }

        public void AddSibling(ProjectItemViewModel newItem, Boolean after)
        {
            if (this.Parent == null)
            {
                return;
            }

            if (after)
            {
                (this.Parent as ProjectItemViewModel).InsertChild(newItem, this.Parent.Children.IndexOf(this) + 1);
            }
            else
            {
                (this.Parent as ProjectItemViewModel).InsertChild(newItem, this.Parent.Children.IndexOf(this));
            }
        }

        protected override void OnSelected()
        {
            ProjectExplorerViewModel.GetInstance().SelectedProjectItem = this;
        }

        protected override void LoadChildren()
        {
            if (!(this.ProjectItem is FolderItem))
            {
                return;
            }

            foreach (ProjectItem child in (this.ProjectItem as FolderItem).Children)
            {
                this.Children.Add(new ProjectItemViewModel(child, this));
            }
        }
    }
    //// End class
}
//// End namespace