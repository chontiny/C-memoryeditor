namespace Ana.View
{
    using Source.Project;
    using Source.Project.ProjectItems;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for ProjectExplorer.xaml
    /// </summary>
    internal partial class ProjectExplorer : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectExplorer" /> class
        /// </summary>
        public ProjectExplorer()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Possible ways the drop may take place
        /// </summary>
        private enum DropModes
        {
            /// <summary>
            /// The item is being dropped above the target
            /// </summary>
            Above,

            /// <summary>
            /// The item is being dropped into the target
            /// </summary>
            Into,

            /// <summary>
            /// The item is being dropped below the target
            /// </summary>
            Below
        }

        /// <summary>
        /// Gets or sets the item being dragged
        /// </summary>
        private ProjectItemViewModel DraggedItem { get; set; }

        /// <summary>
        /// Gets or sets the target item of the drop action
        /// </summary>
        private ProjectItemViewModel Target { get; set; }

        /// <summary>
        /// Gets or sets the drop mode of the drop action
        /// </summary>
        private DropModes DropMode { get; set; }

        /// <summary>
        /// Invoked on a mouse down event in the tree view
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void ProjectExplorerTreeViewMouseDown(Object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !(e.OriginalSource is Image) && !(e.OriginalSource is TextBlock))
            {
                ProjectExplorerViewModel projectExplorerViewModel = this.DataContext as ProjectExplorerViewModel;
                ProjectItemViewModel item = projectExplorerViewModel.SelectedProjectItem as ProjectItemViewModel;

                if (item != null)
                {
                    this.projectExplorerTreeView.Focus();
                    item.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Invoked on a mouse move event in the tree view
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void TreeViewMouseMove(Object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    return;
                }

                this.DraggedItem = this.projectExplorerTreeView.SelectedItem as ProjectItemViewModel;

                DragDropEffects finalDropEffect = DragDrop.DoDragDrop(this.projectExplorerTreeView, this.projectExplorerTreeView.SelectedValue, DragDropEffects.Move);

                if (finalDropEffect != DragDropEffects.Move || this.DraggedItem == this.Target || this.DraggedItem == null || this.Target == null)
                {
                    return;
                }

                // Cannot drop a folder into itself
                if (this.DraggedItem.ContainsChildRecursive(this.Target))
                {
                    return;
                }

                ProjectExplorerViewModel.GetInstance().ProjectRoot.RemoveChildRecursive(this.DraggedItem);

                if (this.DropMode == DropModes.Into)
                {
                    this.Target.AddChild(this.DraggedItem);
                }
                else if (this.DropMode == DropModes.Above)
                {
                    this.Target.AddSibling(this.DraggedItem, after: true);
                }
                else if (this.DropMode == DropModes.Below)
                {
                    this.Target.AddSibling(this.DraggedItem, after: false);
                }

                this.Target = null;
                this.DraggedItem = null;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Invoked a on drag over event in the tree view
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void TreeViewDragOver(Object sender, DragEventArgs e)
        {
            try
            {
                // Verify that this is a valid drop and then store the drop target
                ProjectItemViewModel item = ((sender as TreeViewItem).Header as ProjectItemViewModel) as ProjectItemViewModel;
                if (this.DraggedItem != item)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }

                e.Handled = true;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Invoked on a drop event in the tree view
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void TreeViewDrop(Object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                ProjectItemViewModel targetItem = ((sender as TreeViewItem).Header as ProjectItemViewModel) as ProjectItemViewModel;
                if (targetItem != null && this.DraggedItem != null)
                {
                    const Double ItemHeight = 20.0;
                    const Double DropIntoThreshold = 4.0;
                    Boolean canDropInto = false;

                    if (targetItem.ProjectItem as FolderItem != null)
                    {
                        canDropInto = true;
                    }

                    Double mouseLocationY = e.GetPosition(this).Y;
                    Double targetLocation = (sender as TreeViewItem).TransformToAncestor(this).Transform(new Point(0, 0)).Y + (ItemHeight / 2);

                    if (canDropInto && Math.Abs(mouseLocationY - targetLocation) < DropIntoThreshold)
                    {
                        this.DropMode = DropModes.Into;
                    }
                    else if (mouseLocationY > targetLocation)
                    {
                        this.DropMode = DropModes.Above;
                    }
                    else
                    {
                        this.DropMode = DropModes.Below;
                    }

                    e.Effects = DragDropEffects.Move;
                    this.Target = targetItem;
                }
            }
            catch
            {
            }
        }
    }
    //// End class
}
//// End namespace