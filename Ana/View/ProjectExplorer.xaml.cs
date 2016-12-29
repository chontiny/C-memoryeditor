namespace Ana.View
{
    using Aga.Controls.Tree;
    using Aga.Controls.Tree.NodeControls;
    using Controls;
    using Controls.TreeView;
    using Source.Content;
    using Source.Project;
    using Source.Project.ProjectItems;
    using Source.Utils;
    using Source.Utils.DataStructures;
    using Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Interaction logic for ProjectExplorer.xaml.
    /// </summary>
    internal partial class ProjectExplorer : System.Windows.Controls.UserControl, IProjectExplorerObserver
    {
        private TreeViewAdv projectExplorerTreeView;
        private BiDictionary<ProjectItem, ProjectNode> nodeCache;
        private ProjectRoot projectRoot;
        private TreeModel projectTree;
        private TreeNodeAdv draggedItem;
        private Object accessLock;

        private ToolStripMenuItem addNewItemMenuItem;
        private ToolStripMenuItem deleteSelectionMenuItem;
        private ToolStripMenuItem toggleSelectionMenuItem;
        private ToolStripMenuItem addNewFolderMenuItem;
        private ToolStripMenuItem addNewAddressMenuItem;
        private ToolStripMenuItem addNewScriptMenuItem;
        private ContextMenuStrip contextMenuStrip;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectExplorer" /> class.
        /// </summary>
        public ProjectExplorer()
        {
            this.InitializeComponent();

            this.nodeCache = new BiDictionary<ProjectItem, ProjectNode>();
            this.projectTree = new TreeModel();
            this.accessLock = new Object();

            this.InitializeDesigner();
            this.projectExplorerTreeViewContainer.Children.Add(WinformsHostingHelper.CreateHostedControl(this.projectExplorerTreeView));

            ProjectExplorerViewModel.GetInstance().Subscribe(this);
        }

        public void Update(ProjectRoot projectRoot)
        {
            this.projectRoot = projectRoot;
            this.RebuildProjectStructure();
        }

        public void RebuildProjectStructure()
        {
            this.projectRoot?.BuildParents();

            projectExplorerTreeView.BeginUpdate();
            projectTree.Nodes.Clear();
            nodeCache.Clear();

            if (this.projectRoot != null)
            {
                foreach (ProjectItem Child in this.projectRoot.Children)
                {
                    BuildNodes(Child);
                }
            }

            projectExplorerTreeView.EndUpdate();
            projectExplorerTreeView.ExpandAll();
        }

        public void AddNewAddressItem()
        {
            ProjectExplorerViewModel.GetInstance().AddNewAddressItemCommand.Execute(null);
        }

        public void AddNewScriptItem()
        {
            ProjectExplorerViewModel.GetInstance().AddNewScriptItemCommand.Execute(null);
        }

        public void AddNewFolderItem()
        {
            ProjectExplorerViewModel.GetInstance().AddNewFolderItemCommand.Execute(null);
        }

        private ProjectItem GetSelectedItem()
        {
            Node selectedNode = projectTree.FindNode(projectExplorerTreeView.GetPath(projectExplorerTreeView.SelectedNode));
            ProjectItem SelectedItem = null;

            if (selectedNode != null && typeof(ProjectNode).IsAssignableFrom(selectedNode.GetType()))
            {
                if (nodeCache.Reverse.ContainsKey((ProjectNode)selectedNode))
                {
                    SelectedItem = nodeCache.Reverse[(ProjectNode)selectedNode];
                }
            }

            return SelectedItem;
        }

        private void BuildNodes(ProjectItem projectItem, ProjectItem parent = null)
        {
            if (projectItem == null)
            {
                return;
            }

            Bitmap image = null;

            if (projectItem is AddressItem)
            {
                image = ImageUtils.BitmapImageToBitmap(Images.CollectValues);
            }
            else if (projectItem is ScriptItem)
            {
                image = ImageUtils.BitmapImageToBitmap(Images.CollectValues);
            }
            else if (projectItem is FolderItem)
            {
                image = ImageUtils.BitmapImageToBitmap(Images.Open);
            }

            image?.MakeTransparent();

            // Create new node to insert
            ProjectNode projectNode = new ProjectNode(projectItem.Description);
            projectNode.ProjectItem = projectItem;
            projectNode.EntryIcon = image;
            projectNode.IsChecked = projectItem.IsActivated;

            if (parent != null && nodeCache.ContainsKey(parent))
            {
                nodeCache[parent].Nodes.Add(projectNode);
            }
            else
            {
                projectTree.Nodes.Add(projectNode);
            }

            nodeCache.Add(projectItem, projectNode);

            if (projectItem is FolderItem)
            {
                FolderItem folderItem = projectItem as FolderItem;

                foreach (ProjectItem child in folderItem.Children)
                {
                    BuildNodes(child, projectItem);
                }
            }
        }

        private ProjectNode GetProjectNodeFromTreeNodeAdv(TreeNodeAdv treeNodeAdv)
        {
            Node node = projectTree.FindNode(projectExplorerTreeView.GetPath(treeNodeAdv));

            if (node == null || !typeof(ProjectNode).IsAssignableFrom(node.GetType()))
            {
                return null;
            }

            return node as ProjectNode;
        }

        private ProjectItem GetProjectItemFromNode(TreeNodeAdv treeNodeAdv)
        {
            return GetProjectNodeFromTreeNodeAdv(treeNodeAdv)?.ProjectItem;
        }

        private void EntryDescriptionDrawText(Object sender, DrawEventArgs e)
        {
            e.TextColor = DarkBrushes.BaseColor2;
        }

        private void ActivateSelectedItems()
        {
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                return;
            }

            List<ProjectItem> nodes = new List<ProjectItem>();
            projectExplorerTreeView.SelectedNodes.ForEach(x => nodes.Add(GetProjectItemFromNode(x)));

            if (nodes.Count <= 0)
            {
                return;
            }

            // Behavior here is undefined, we could check only the selected items, or enforce the recursive rules of folders
            nodes.ForEach(x => CheckItem(x, !x.IsActivated));
            projectExplorerTreeView.SelectedNodes.ForEach(x => nodeCache[GetProjectItemFromNode(x)].IsChecked = GetProjectItemFromNode(x).IsActivated);

            RebuildProjectStructure();
        }

        private void DeleteSelectedItems()
        {
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                return;
            }

            List<ProjectItem> Nodes = new List<ProjectItem>();
            projectExplorerTreeView.SelectedNodes.ForEach(x => Nodes.Add(GetProjectItemFromNode(x)));
            // projectExplorerPresenter.DeleteProjectItems(Nodes);

            this.RebuildProjectStructure();
        }

        private void CheckItem(ProjectItem projectItem, Boolean activated)
        {
            if (projectItem == null)
            {
                return;
            }

            projectItem.IsActivated = activated;
            nodeCache[projectItem].IsChecked = projectItem.IsActivated;

            if (projectItem is FolderItem)
            {
                FolderItem folderItem = projectItem as FolderItem;

                foreach (ProjectItem child in folderItem.Children)
                {
                    this.CheckItem(child, activated);
                }
            }
        }

        private void ProjectExplorerTreeViewKeyPress(Object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != ' ')
            {
                return;
            }

            this.ActivateSelectedItems();
        }

        private void ProjectContextMenuStrip_Opening(Object sender, CancelEventArgs e)
        {
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                deleteSelectionMenuItem.Enabled = false;
                toggleSelectionMenuItem.Enabled = false;
            }
            else
            {
                deleteSelectionMenuItem.Enabled = true;
                toggleSelectionMenuItem.Enabled = true;
            }
        }

        private void CheckIndex(Object sender, NodeControlValueEventArgs e)
        {
            if (e == null || e.Node == null)
            {
                return;
            }

            ProjectItem projectItem = GetProjectItemFromNode(e.Node);

            if (projectItem == null)
            {
                return;
            }

            this.CheckItem(projectItem, !projectItem.IsActivated);

            this.RebuildProjectStructure();
        }

        private void ProjectExplorerTreeViewNodeMouseDoubleClick(Object sender, TreeNodeAdvMouseEventArgs e)
        {
            if (e == null || e.Node == null)
            {
                return;
            }

            ProjectItem projectItem = GetProjectItemFromNode(e.Node);
            this.CheckItem(projectItem, !projectItem.IsActivated);
        }

        private void ProjectExplorerTreeViewSelectionChanged(Object sender, EventArgs e)
        {
            List<TreeNodeAdv> treeNodes = new List<TreeNodeAdv>();
            List<ProjectItem> projectItems = new List<ProjectItem>();

            projectExplorerTreeView.SelectedNodes.ForEach(x => treeNodes.Add(x));
            treeNodes.ForEach(x => projectItems.Add(GetProjectItemFromNode(x)));

            ProjectExplorerViewModel.GetInstance().SelectedProjectItem = (projectItems?.Count ?? 0) > 0 ? projectItems.First() : null;
        }

        private void ProjectExplorerTreeView_ItemDrag(Object sender, ItemDragEventArgs e)
        {
            draggedItem = e.Item as TreeNodeAdv;
            this.projectExplorerTreeView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ProjectExplorerTreeViewDragOver(Object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ProjectExplorerTreeViewDragEnter(Object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ProjectExplorerTreeViewDragDrop(Object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the drop location.
            Point targetPoint = projectExplorerTreeView.PointToClient(new Point(e.X, e.Y));

            // Retrieve the node at the drop location.
            TreeNodeAdv targetNodeAdv = projectExplorerTreeView.GetNodeAt(targetPoint);

            // Retrieve the node that was dragged.
            TreeNodeAdv[] draggedNodesAdv = e.Data.GetData(typeof(TreeNodeAdv[])) as TreeNodeAdv[];

            if (draggedNodesAdv.Count() <= 0)
            {
                return;
            }

            TreeNodeAdv draggedNodeAdv = draggedNodesAdv[0];

            if (draggedNodeAdv != null && targetNodeAdv != null && draggedNodeAdv != targetNodeAdv)
            {
                ProjectItem draggedItem = GetProjectItemFromNode(draggedNodeAdv);
                ProjectItem targetItem = GetProjectItemFromNode(targetNodeAdv);

                this.projectRoot?.BuildParents();

                if (!(draggedItem is FolderItem) || !(draggedItem as FolderItem).HasNode(targetItem))
                {
                    switch (projectExplorerTreeView.DropPosition.Position)
                    {
                        case NodePosition.Before:
                            projectRoot.RemoveNode(draggedItem);
                            targetItem.AddSibling(draggedItem, after: false);
                            break;
                        case NodePosition.Inside:
                            if (targetItem is FolderItem)
                            {
                                projectRoot.RemoveNode(draggedItem);
                                (targetItem as FolderItem).AddChild(draggedItem);
                            }
                            break;
                        case NodePosition.After:
                            projectRoot.RemoveNode(draggedItem);
                            targetItem.AddSibling(draggedItem, after: true);
                            break;
                    }
                }

                this.RebuildProjectStructure();
            }
        }

        /// <summary>
        /// Initialize all windows forms components.
        /// </summary>
        private void InitializeDesigner()
        {
            NodeCheckBox entryCheckBox = new NodeCheckBox();
            entryCheckBox.DataPropertyName = "IsChecked";
            entryCheckBox.EditEnabled = true;
            entryCheckBox.LeftMargin = 0;
            entryCheckBox.ParentColumn = null;
            entryCheckBox.IsEditEnabledValueNeeded += CheckIndex;

            NodeIcon entryIcon = new NodeIcon();
            entryIcon.DataPropertyName = "EntryIcon";
            entryIcon.LeftMargin = 1;
            entryIcon.ParentColumn = null;
            entryIcon.ScaleMode = ImageScaleMode.Clip;

            NodeTextBox entryDescription = new NodeTextBox();
            entryDescription.DataPropertyName = "EntryDescription";
            entryDescription.IncrementalSearchEnabled = true;
            entryDescription.LeftMargin = 3;
            entryDescription.ParentColumn = null;
            entryDescription.DrawText += EntryDescriptionDrawText;

            this.toggleSelectionMenuItem = new ToolStripMenuItem("Toggle Selection");
            this.addNewItemMenuItem = new ToolStripMenuItem("Add New...");
            this.deleteSelectionMenuItem = new ToolStripMenuItem("Delete Selection");
            this.addNewFolderMenuItem = new ToolStripMenuItem("Add Folder", ImageUtils.BitmapImageToBitmap(Images.Open));
            this.addNewAddressMenuItem = new ToolStripMenuItem("Add Address", ImageUtils.BitmapImageToBitmap(Images.CollectValues));
            this.addNewScriptMenuItem = new ToolStripMenuItem("Add Script", ImageUtils.BitmapImageToBitmap(Images.CollectValues));
            this.contextMenuStrip = new ContextMenuStrip();

            this.toggleSelectionMenuItem.Click += ToggleSelectionMenuItem_Click;
            this.deleteSelectionMenuItem.Click += DeleteSelectionMenuItem_Click;
            this.addNewFolderMenuItem.Click += AddNewFolderMenuItem_Click;
            this.addNewAddressMenuItem.Click += AddNewAddressMenuItem_Click;
            this.addNewScriptMenuItem.Click += AddNewScriptMenuItem_Click;

            this.addNewItemMenuItem.DropDownItems.Add(addNewFolderMenuItem);
            this.addNewItemMenuItem.DropDownItems.Add(addNewAddressMenuItem);
            this.addNewItemMenuItem.DropDownItems.Add(addNewScriptMenuItem);

            this.contextMenuStrip.Items.Add(toggleSelectionMenuItem);
            this.contextMenuStrip.Items.Add(addNewItemMenuItem);
            this.contextMenuStrip.Items.Add(deleteSelectionMenuItem);

            this.projectExplorerTreeView = new TreeViewAdv();
            this.projectExplorerTreeView.NodeControls.Add(entryCheckBox);
            this.projectExplorerTreeView.NodeControls.Add(entryIcon);
            this.projectExplorerTreeView.NodeControls.Add(entryDescription);
            this.projectExplorerTreeView.SelectionMode = TreeSelectionMode.Multi;
            this.projectExplorerTreeView.BorderStyle = BorderStyle.None;
            this.projectExplorerTreeView.Model = this.projectTree;
            this.projectExplorerTreeView.AllowDrop = true;
            this.projectExplorerTreeView.FullRowSelect = true;
            this.projectExplorerTreeView.ContextMenuStrip = contextMenuStrip;

            this.projectExplorerTreeView.ItemDrag += this.ProjectExplorerTreeView_ItemDrag;
            this.projectExplorerTreeView.NodeMouseDoubleClick += this.ProjectExplorerTreeViewNodeMouseDoubleClick;
            this.projectExplorerTreeView.SelectionChanged += this.ProjectExplorerTreeViewSelectionChanged;
            this.projectExplorerTreeView.DragDrop += this.ProjectExplorerTreeViewDragDrop;
            this.projectExplorerTreeView.DragEnter += this.ProjectExplorerTreeViewDragEnter;
            this.projectExplorerTreeView.DragOver += this.ProjectExplorerTreeViewDragOver;
            this.projectExplorerTreeView.KeyPress += this.ProjectExplorerTreeViewKeyPress;

            this.projectExplorerTreeView.BackColor = DarkBrushes.BaseColor3;
            this.projectExplorerTreeView.ForeColor = DarkBrushes.BaseColor2;
            this.projectExplorerTreeView.DragDropMarkColor = DarkBrushes.BaseColor11;
            this.projectExplorerTreeView.LineColor = DarkBrushes.BaseColor11;
        }

        private void AddNewScriptMenuItem_Click(Object sender, EventArgs e)
        {
            this.AddNewScriptItem();
        }

        private void AddNewAddressMenuItem_Click(Object sender, EventArgs e)
        {
            this.AddNewAddressItem();
        }

        private void AddNewFolderMenuItem_Click(Object sender, EventArgs e)
        {
            this.AddNewFolderItem();
        }

        private void DeleteSelectionMenuItem_Click(Object sender, EventArgs e)
        {
            this.DeleteSelectedItems();
        }

        private void ToggleSelectionMenuItem_Click(Object sender, EventArgs e)
        {
            this.ActivateSelectedItems();
        }
    }
    //// End class
}
//// End namespace