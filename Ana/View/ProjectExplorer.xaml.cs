namespace Ana.View
{
    using Aga.Controls.Tree;
    using Aga.Controls.Tree.NodeControls;
    using Controls;
    using Controls.TreeView;
    using Source.Content;
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
    /// Interaction logic for ProjectExplorer.xaml
    /// </summary>
    internal partial class ProjectExplorer : System.Windows.Controls.UserControl
    {
        private TreeViewAdv projectExplorerTreeView;
        private BiDictionary<ProjectItem, ProjectNode> nodeCache;
        private TreeModel projectTree;
        private TreeNodeAdv draggedItem;
        private Object accessLock;

        private FolderItem projectRoot;

        private NodeCheckBox EntryCheckBox;
        private NodeIcon EntryIcon;
        private NodeTextBox EntryDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectExplorer" /> class
        /// </summary>
        public ProjectExplorer()
        {
            this.InitializeComponent();

            this.EntryCheckBox = new NodeCheckBox();
            this.EntryCheckBox.DataPropertyName = "IsChecked";
            this.EntryCheckBox.EditEnabled = true;
            this.EntryCheckBox.LeftMargin = 0;
            this.EntryCheckBox.ParentColumn = null;
            this.EntryCheckBox.IsEditEnabledValueNeeded += CheckIndex;

            this.EntryIcon = new NodeIcon();
            this.EntryIcon.DataPropertyName = "EntryIcon";
            this.EntryIcon.LeftMargin = 1;
            this.EntryIcon.ParentColumn = null;
            this.EntryIcon.ScaleMode = ImageScaleMode.Clip;

            this.EntryDescription = new NodeTextBox();
            this.EntryDescription.DataPropertyName = "EntryDescription";
            this.EntryDescription.IncrementalSearchEnabled = true;
            this.EntryDescription.LeftMargin = 3;
            this.EntryDescription.ParentColumn = null;
            this.EntryDescription.DrawText += EntryDescriptionDrawText;

            this.nodeCache = new BiDictionary<ProjectItem, ProjectNode>();
            this.projectTree = new TreeModel();
            this.accessLock = new Object();

            this.projectExplorerTreeView = new TreeViewAdv();
            this.projectExplorerTreeView.NodeControls.Add(this.EntryCheckBox);
            this.projectExplorerTreeView.NodeControls.Add(this.EntryIcon);
            this.projectExplorerTreeView.NodeControls.Add(this.EntryDescription);
            this.projectExplorerTreeView.BorderStyle = BorderStyle.None;
            this.projectExplorerTreeView.Model = this.projectTree;
            this.projectExplorerTreeView.AllowDrop = true;

            this.projectExplorerTreeView.ItemDrag += this.ProjectExplorerTreeView_ItemDrag;
            this.projectExplorerTreeView.NodeMouseDoubleClick += this.ProjectExplorerTreeView_NodeMouseDoubleClick;
            this.projectExplorerTreeView.SelectionChanged += this.ProjectExplorerTreeView_SelectionChanged;
            this.projectExplorerTreeView.DragDrop += this.ProjectExplorerTreeView_DragDrop;
            this.projectExplorerTreeView.DragEnter += this.ProjectExplorerTreeView_DragEnter;
            this.projectExplorerTreeView.DragOver += this.ProjectExplorerTreeView_DragOver;
            this.projectExplorerTreeView.KeyPress += this.ProjectExplorerTreeView_KeyPress;

            this.projectExplorerTreeView.BackColor = DarkBrushes.BaseColor3;
            this.projectExplorerTreeView.ForeColor = DarkBrushes.BaseColor2;
            this.projectExplorerTreeView.DragDropMarkColor = DarkBrushes.BaseColor11;
            this.projectExplorerTreeView.LineColor = DarkBrushes.BaseColor2;

            this.projectExplorerTreeViewContainer.Children.Add(WinformsHostingHelper.CreateHostedControl(this.projectExplorerTreeView));

            // Testing
            FolderItem nice = new FolderItem();
            FolderItem nice2 = new FolderItem();
            FolderItem nice3 = new FolderItem();
            FolderItem nice4 = new FolderItem();
            FolderItem nicea = new FolderItem();
            FolderItem niceb = new FolderItem();
            FolderItem nicec = new FolderItem();
            nice.Children.Add(nice2);
            nice.Children.Add(nicea);
            nice.Children.Add(niceb);
            nice.Children.Add(nicec);
            nice2.Children.Add(nice3);
            nice3.Children.Add(nice4);

            this.projectRoot = nice;

            RebuildProjectStructure(nice);
        }

        private void EntryDescriptionDrawText(Object sender, DrawEventArgs e)
        {
            e.TextColor = DarkBrushes.BaseColor2;
        }

        public void RebuildProjectStructure(FolderItem projectRoot)
        {
            this.projectRoot = projectRoot;

            projectExplorerTreeView.BeginUpdate();
            projectTree.Nodes.Clear();
            nodeCache.Clear();

            if (projectRoot != null)
            {
                foreach (ProjectItem Child in projectRoot.Children)
                {
                    BuildNodes(Child);
                }
            }

            projectExplorerTreeView.EndUpdate();
            projectExplorerTreeView.ExpandAll();
        }

        private void BuildNodes(ProjectItem projectItem, ProjectItem parent = null)
        {
            if (projectItem == null)
                return;

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

        public void AddNewAddressItem()
        {
            // projectExplorerPresenter.AddNewAddressItem(GetSelectedItem());
        }

        public void AddNewScriptItem()
        {
            // projectExplorerPresenter.AddNewScriptItem(GetSelectedItem());
        }

        public void AddNewFolderItem()
        {
            // projectExplorerPresenter.AddNewFolderItem(GetSelectedItem());
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
            // Nodes.ForEach(x => DoCheck(x, !x.GetActivationState()));
            // projectExplorerPresenter.ActivateProjectItems(nodes, !nodes.First().GetActivationState());
            projectExplorerTreeView.SelectedNodes.ForEach(x => nodeCache[GetProjectItemFromNode(x)].IsChecked = GetProjectItemFromNode(x).IsActivated);

            RebuildProjectStructure(projectRoot);
        }

        private void DeleteSelectedItems()
        {
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
                return;

            List<ProjectItem> Nodes = new List<ProjectItem>();
            projectExplorerTreeView.SelectedNodes.ForEach(x => Nodes.Add(GetProjectItemFromNode(x)));
            // projectExplorerPresenter.DeleteProjectItems(Nodes);

            RebuildProjectStructure(projectRoot);
        }

        private void DoCheck(ProjectItem projectItem, Boolean activated)
        {
            if (projectItem == null)
            {
                return;
            }

            // projectExplorerPresenter.ActivateProjectItem(projectItem, activated);
            nodeCache[projectItem].IsChecked = projectItem.IsActivated;

            if (projectItem is FolderItem)
            {
                FolderItem folderItem = projectItem as FolderItem;

                foreach (ProjectItem child in folderItem.Children)
                {
                    DoCheck(child, activated);
                }
            }
        }

        private void AddressToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            AddNewAddressItem();
        }

        private void AddNewScriptToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            AddNewScriptItem();
        }

        private void FolderToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            AddNewFolderItem();
        }

        private void AddressRightClickToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            AddNewAddressItem();
        }

        private void ScriptRightClickToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            AddNewScriptItem();
        }

        private void FolderRightClickToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            AddNewFolderItem();
        }

        private void DeleteSelectionToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }

        private void ToggleFreezeToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            ActivateSelectedItems();
        }

        private void ProjectExplorerTreeView_KeyPress(Object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != ' ')
            {
                return;
            }

            ActivateSelectedItems();
        }

        private void ProjectContextMenuStrip_Opening(Object sender, CancelEventArgs e)
        {
            /*
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                ToggleActivationToolStripMenuItem.Enabled = false;
                DeleteSelectionToolStripMenuItem.Enabled = false;
                AddNewItemToolStripMenuItem.Enabled = true;
            }
            else
            {
                ToggleActivationToolStripMenuItem.Enabled = true;
                DeleteSelectionToolStripMenuItem.Enabled = true;
                AddNewItemToolStripMenuItem.Enabled = true;
            }*/
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

            DoCheck(projectItem, !projectItem.IsActivated);

            RebuildProjectStructure(projectRoot);
        }

        private void ProjectExplorerTreeView_NodeMouseDoubleClick(Object sender, TreeNodeAdvMouseEventArgs e)
        {
            if (e == null || e.Node == null)
                return;

            ProjectItem projectItem = GetProjectItemFromNode(e.Node);
            // projectExplorerPresenter.PerformDefaultAction(projectItem);
        }

        private void ProjectExplorerTreeView_SelectionChanged(Object sender, EventArgs e)
        {
            List<TreeNodeAdv> treeNodes = new List<TreeNodeAdv>();
            List<ProjectItem> projectItems = new List<ProjectItem>();

            projectExplorerTreeView.SelectedNodes.ForEach(X => treeNodes.Add(X));
            treeNodes.ForEach(x => projectItems.Add(GetProjectItemFromNode(x)));

            // projectExplorerPresenter.UpdateSelection(projectItems);
        }

        private void ProjectExplorerTreeView_ItemDrag(Object sender, ItemDragEventArgs e)
        {
            draggedItem = e.Item as TreeNodeAdv;
            this.projectExplorerTreeView.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ProjectExplorerTreeView_DragOver(Object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ProjectExplorerTreeView_DragEnter(Object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ProjectExplorerTreeView_DragDrop(Object sender, DragEventArgs e)
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

                /*
                switch (projectExplorerTreeView.DropPosition.Position)
                {
                    case NodePosition.Before:
                        if (!draggedItem.HasNode(targetItem))
                        {
                            projectRoot.RemoveNode(draggedItem);
                            targetItem.AddSibling(draggedItem, false);
                        }
                        break;
                    case NodePosition.Inside:
                        if (!draggedItem.HasNode(targetItem))
                        {
                            projectRoot.RemoveNode(draggedItem);
                            targetItem.AddChild(draggedItem);
                        }
                        break;
                    case NodePosition.After:
                        if (!draggedItem.HasNode(targetItem))
                        {
                            projectRoot.RemoveNode(draggedItem);
                            targetItem.AddSibling(draggedItem, true);
                        }
                        break;
                }
                */

                RebuildProjectStructure(projectRoot);
            }
        }
    }
    //// End class
}
//// End namespace