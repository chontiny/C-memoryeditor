namespace Ana.View
{
    using Aga.Controls.Tree;
    using Aga.Controls.Tree.NodeControls;
    using Controls;
    using Controls.TreeView;
    using SharpDX.DirectInput;
    using Source.Content;
    using Source.Engine;
    using Source.Engine.Input.Keyboard;
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
    internal partial class ProjectExplorer : System.Windows.Controls.UserControl, IProjectExplorerObserver, IKeyboardObserver
    {
        private const char DeleteKeyCode = (char)0x127;

        private TreeViewAdv projectExplorerTreeView;
        private BiDictionary<ProjectItem, ProjectNode> nodeCache;
        private ProjectRoot projectRoot;
        private TreeModel projectTree;
        private TreeNodeAdv draggedItem;
        private Object accessLock;

        private IEnumerable<ProjectItem> clipBoard;

        private ToolStripMenuItem addNewItemMenuItem;
        private ToolStripMenuItem deleteSelectionMenuItem;
        private ToolStripMenuItem toggleSelectionMenuItem;
        private ToolStripMenuItem compileSelectionMenuItem;
        private ToolStripMenuItem copySelectionMenuItem;
        private ToolStripMenuItem cutSelectionMenuItem;
        private ToolStripMenuItem pasteSelectionMenuItem;
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

            EngineCore.GetInstance().Input?.GetKeyboardCapture().Subscribe(this);
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

            this.projectExplorerTreeView.BeginUpdate();
            this.projectTree.Nodes.Clear();
            this.nodeCache.Clear();

            if (this.projectRoot != null)
            {
                foreach (ProjectItem child in this.projectRoot.Children.ToArray())
                {
                    this.BuildNodes(child);
                }
            }

            this.projectExplorerTreeView.EndUpdate();
            this.projectExplorerTreeView.ExpandAll();
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

        private void CopySelection()
        {
            this.clipBoard = this.CloneSelectedProjectItems();
        }

        private void CutSelection()
        {
            this.clipBoard = this.CloneSelectedProjectItems();
            this.DeleteSelectedItems();
        }

        private void PasteSelection()
        {
            if (this.clipBoard == null || this.clipBoard.Count() <= 0)
            {
                return;
            }

            foreach (ProjectItem projectItem in clipBoard)
            {
                // We must clone the item, such as to prevent duplicate references of the same exact object
                ProjectExplorerViewModel.GetInstance().AddNewProjectItems(true, projectItem.Clone());
            }
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

                foreach (ProjectItem child in folderItem.Children.ToArray())
                {
                    this.BuildNodes(child, projectItem);
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

        private IEnumerable<ProjectItem> GetSelectedProjectItems()
        {
            List<ProjectItem> nodes = new List<ProjectItem>();
            projectExplorerTreeView.SelectedNodes.ForEach(x => nodes.Add(this.GetProjectItemFromNode(x)));
            return nodes;
        }

        private IEnumerable<ProjectItem> CloneSelectedProjectItems()
        {
            List<ProjectItem> nodes = new List<ProjectItem>();
            projectExplorerTreeView.SelectedNodes.ForEach(x => nodes.Add(this.GetProjectItemFromNode(x).Clone()));
            return nodes;
        }

        private ProjectItem GetProjectItemFromNode(TreeNodeAdv treeNodeAdv)
        {
            return this.GetProjectNodeFromTreeNodeAdv(treeNodeAdv)?.ProjectItem;
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

            IEnumerable<ProjectItem> selectedProjectItems = this.GetSelectedProjectItems();

            if (selectedProjectItems == null || selectedProjectItems.Count() <= 0)
            {
                return;
            }

            // Behavior here is undefined, we could check only the selected items, or enforce the recursive rules of folders
            selectedProjectItems.ForEach(x => CheckItem(x, !x.IsActivated));
            projectExplorerTreeView.SelectedNodes.ForEach(x => nodeCache[GetProjectItemFromNode(x)].IsChecked = GetProjectItemFromNode(x).IsActivated);

            this.RebuildProjectStructure();
        }

        private void CompileSelectedItems()
        {
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                return;
            }

            IEnumerable<ProjectItem> selectedProjectItems = this.GetSelectedProjectItems();

            if (selectedProjectItems == null || selectedProjectItems.Count() <= 0)
            {
                return;
            }

            foreach (ProjectItem projectItem in selectedProjectItems)
            {
                if (projectItem is ScriptItem)
                {
                    ScriptItem compiledScript = (projectItem as ScriptItem)?.Compile();

                    if (compiledScript != null)
                    {
                        ProjectExplorerViewModel.GetInstance().AddNewProjectItems(true, compiledScript);
                    }
                }
            }
        }

        private void DeleteSelectedItems()
        {
            if (projectExplorerTreeView.SelectedNodes == null || projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                return;
            }

            System.Windows.MessageBoxResult result =
                MessageBoxEx.Show(System.Windows.Application.Current.MainWindow,
                "Delete selected items?",
                "Confirm",
                System.Windows.MessageBoxButton.OKCancel,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.OK)
            {
                ProjectExplorerViewModel.GetInstance().DeleteSelectionCommand.Execute(null);
            }
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
                foreach (ProjectItem child in (projectItem as FolderItem).Children.ToArray())
                {
                    this.CheckItem(child, activated);
                }
            }
        }

        public void OnKeyPress(Key key)
        {
            switch (key)
            {
                case Key.Space:
                    this.ActivateSelectedItems();
                    break;
                case Key.Delete:
                    this.DeleteSelectedItems();
                    break;
                case Key.C:
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        this.CopySelection();
                    }
                    break;
                case Key.X:
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        this.CutSelection();
                    }
                    break;
                case Key.V:
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        this.PasteSelection();
                    }
                    break;
            }
        }

        public void OnKeyRelease(Key key)
        {
        }

        public void OnKeyDown(Key key)
        {
        }

        public void OnUpdateAllDownKeys(HashSet<Key> pressedKeys)
        {
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

            this.projectExplorerTreeView.SelectedNodes.ForEach(x => treeNodes.Add(x));
            treeNodes.ForEach(x => projectItems.Add(GetProjectItemFromNode(x)));

            ProjectExplorerViewModel.GetInstance().SelectedProjectItems = projectItems;
        }

        private void ProjectExplorerTreeViewItemDrag(Object sender, ItemDragEventArgs e)
        {
            this.draggedItem = e.Item as TreeNodeAdv;
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
                            targetItem.Parent?.AddSibling(targetItem, draggedItem, after: false);
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
                            targetItem.Parent?.AddSibling(targetItem, draggedItem, after: true);
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

            this.toggleSelectionMenuItem = new ToolStripMenuItem("Toggle");
            this.compileSelectionMenuItem = new ToolStripMenuItem("Compile");
            this.addNewItemMenuItem = new ToolStripMenuItem("Add New...");
            this.deleteSelectionMenuItem = new ToolStripMenuItem("Delete");
            this.copySelectionMenuItem = new ToolStripMenuItem("Copy");
            this.cutSelectionMenuItem = new ToolStripMenuItem("Cut");
            this.pasteSelectionMenuItem = new ToolStripMenuItem("Paste");
            this.addNewFolderMenuItem = new ToolStripMenuItem("Add Folder", ImageUtils.BitmapImageToBitmap(Images.Open));
            this.addNewAddressMenuItem = new ToolStripMenuItem("Add Address", ImageUtils.BitmapImageToBitmap(Images.CollectValues));
            this.addNewScriptMenuItem = new ToolStripMenuItem("Add Script", ImageUtils.BitmapImageToBitmap(Images.CollectValues));
            this.contextMenuStrip = new ContextMenuStrip();

            KeysConverter KeysConverter = new KeysConverter();
            this.toggleSelectionMenuItem.ShortcutKeyDisplayString = KeysConverter.ConvertToString(Keys.Space);
            this.copySelectionMenuItem.ShortcutKeyDisplayString = KeysConverter.ConvertToString(Keys.Control | Keys.C);
            this.cutSelectionMenuItem.ShortcutKeyDisplayString = KeysConverter.ConvertToString(Keys.Control | Keys.X);
            this.pasteSelectionMenuItem.ShortcutKeyDisplayString = KeysConverter.ConvertToString(Keys.Control | Keys.V);
            this.deleteSelectionMenuItem.ShortcutKeyDisplayString = KeysConverter.ConvertToString(Keys.Delete);

            this.toggleSelectionMenuItem.Click += ToggleSelectionMenuItemClick;
            this.compileSelectionMenuItem.Click += CompileSelectionMenuItemClick;
            this.deleteSelectionMenuItem.Click += DeleteSelectionMenuItemClick;
            this.copySelectionMenuItem.Click += CopySelectionMenuItemClick;
            this.cutSelectionMenuItem.Click += CutSelectionMenuItemClick;
            this.pasteSelectionMenuItem.Click += PasteSelectionMenuItemClick;
            this.addNewFolderMenuItem.Click += AddNewFolderMenuItemClick;
            this.addNewAddressMenuItem.Click += AddNewAddressMenuItemClick;
            this.addNewScriptMenuItem.Click += AddNewScriptMenuItemClick;
            this.contextMenuStrip.Opening += ContextMenuStripOpening;

            this.addNewItemMenuItem.DropDownItems.Add(addNewFolderMenuItem);
            this.addNewItemMenuItem.DropDownItems.Add(addNewAddressMenuItem);
            this.addNewItemMenuItem.DropDownItems.Add(addNewScriptMenuItem);

            this.contextMenuStrip.Items.Add(toggleSelectionMenuItem);
            this.contextMenuStrip.Items.Add(compileSelectionMenuItem);
            this.contextMenuStrip.Items.Add(addNewItemMenuItem);
            this.contextMenuStrip.Items.Add(deleteSelectionMenuItem);
            this.contextMenuStrip.Items.Add(new ToolStripSeparator());
            this.contextMenuStrip.Items.Add(copySelectionMenuItem);
            this.contextMenuStrip.Items.Add(cutSelectionMenuItem);
            this.contextMenuStrip.Items.Add(pasteSelectionMenuItem);

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

            this.projectExplorerTreeView.ItemDrag += this.ProjectExplorerTreeViewItemDrag;
            this.projectExplorerTreeView.NodeMouseDoubleClick += this.ProjectExplorerTreeViewNodeMouseDoubleClick;
            this.projectExplorerTreeView.SelectionChanged += this.ProjectExplorerTreeViewSelectionChanged;
            this.projectExplorerTreeView.DragDrop += this.ProjectExplorerTreeViewDragDrop;
            this.projectExplorerTreeView.DragEnter += this.ProjectExplorerTreeViewDragEnter;
            this.projectExplorerTreeView.DragOver += this.ProjectExplorerTreeViewDragOver;

            this.projectExplorerTreeView.BackColor = DarkBrushes.BaseColor3;
            this.projectExplorerTreeView.ForeColor = DarkBrushes.BaseColor2;
            this.projectExplorerTreeView.DragDropMarkColor = DarkBrushes.BaseColor11;
            this.projectExplorerTreeView.LineColor = DarkBrushes.BaseColor11;
        }

        private void ContextMenuStripOpening(Object sender, CancelEventArgs e)
        {
            if (this.projectExplorerTreeView.SelectedNodes == null || this.projectExplorerTreeView.SelectedNodes.Count <= 0)
            {
                this.deleteSelectionMenuItem.Enabled = false;
                this.toggleSelectionMenuItem.Enabled = false;
                this.copySelectionMenuItem.Enabled = false;
                this.cutSelectionMenuItem.Enabled = false;
                this.compileSelectionMenuItem.Visible = false;
            }
            else
            {
                this.deleteSelectionMenuItem.Enabled = true;
                this.toggleSelectionMenuItem.Enabled = true;
                this.copySelectionMenuItem.Enabled = true;
                this.cutSelectionMenuItem.Enabled = true;

                if (this.projectExplorerTreeView.SelectedNodes.All(x => this.GetProjectItemFromNode(x)?.GetType() == typeof(ScriptItem)))
                {
                    this.compileSelectionMenuItem.Visible = true;
                }
                else
                {
                    this.compileSelectionMenuItem.Visible = false;
                }
            }

            if (this.clipBoard == null || this.clipBoard.Count() <= 0)
            {
                this.pasteSelectionMenuItem.Enabled = false;
            }
            else
            {
                this.pasteSelectionMenuItem.Enabled = true;
            }
        }

        private void CopySelectionMenuItemClick(Object sender, EventArgs e)
        {
            this.CopySelection();
        }

        private void CutSelectionMenuItemClick(Object sender, EventArgs e)
        {
            this.CutSelection();
        }

        private void PasteSelectionMenuItemClick(Object sender, EventArgs e)
        {
            this.PasteSelection();
        }

        private void AddNewScriptMenuItemClick(Object sender, EventArgs e)
        {
            this.AddNewScriptItem();
        }

        private void AddNewAddressMenuItemClick(Object sender, EventArgs e)
        {
            this.AddNewAddressItem();
        }

        private void AddNewFolderMenuItemClick(Object sender, EventArgs e)
        {
            this.AddNewFolderItem();
        }

        private void DeleteSelectionMenuItemClick(Object sender, EventArgs e)
        {
            this.DeleteSelectedItems();
        }

        private void ToggleSelectionMenuItemClick(Object sender, EventArgs e)
        {
            this.ActivateSelectedItems();
        }

        private void CompileSelectionMenuItemClick(Object sender, EventArgs e)
        {
            this.CompileSelectedItems();
        }
    }
    //// End class
}
//// End namespace