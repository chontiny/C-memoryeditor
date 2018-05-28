namespace Squalr.Source.SolutionExplorer
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Properties;
    using Squalr.Source.Docking;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;

    public class SolutionExplorerViewModel : ToolViewModel
    {
        /// <summary>
        /// The filter to use for saving and loading project filters.
        /// </summary>
        public const String ProjectExtensionFilter = "Solution File (*.sln)|*.sln|";

        /// <summary>
        /// The file extension for project items.
        /// </summary>
        private const String ProjectFileExtension = ".sln";

        /// <summary>
        /// Singleton instance of the <see cref="ProjectExplorerViewModel" /> class.
        /// </summary>
        private static Lazy<SolutionExplorerViewModel> solutionExplorerViewModelInstance = new Lazy<SolutionExplorerViewModel>(
                () => { return new SolutionExplorerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private DirInfo currentTreeItem;

        private DirInfo currentDirectory;

        private FullyObservableCollection<DirInfo> systemDirectorySource;

        private FullyObservableCollection<DirInfo> currentItems;

        public SolutionExplorerViewModel() : base("Solution Explorer")
        {
            this.SetProjectRootCommand = new RelayCommand(() => this.SetProjectRoot());
            this.OpenProjectCommand = new RelayCommand(() => this.OpenProject());
            this.NewProjectCommand = new RelayCommand(() => this.NewProject());

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="SolutionExplorerViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static SolutionExplorerViewModel GetInstance()
        {
            return SolutionExplorerViewModel.solutionExplorerViewModelInstance.Value;
        }

        /// <summary>
        /// Gets the command to set the project root.
        /// </summary>
        public ICommand SetProjectRootCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a project.
        /// </summary>
        public ICommand OpenProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to create a new project.
        /// </summary>
        public ICommand NewProjectCommand { get; private set; }

        /// <summary>
        /// List of the directories.
        /// </summary>
        public FullyObservableCollection<DirInfo> SystemDirectorySource
        {
            get
            {
                return systemDirectorySource;
            }
            set
            {
                systemDirectorySource = value;
                RaisePropertyChanged(nameof(this.SystemDirectorySource));
            }
        }

        /// <summary>
        /// Current selected item in the tree.
        /// </summary>
        public DirInfo CurrentTreeItem
        {
            get
            {
                return currentTreeItem;
            }
            set
            {
                currentTreeItem = value;
                this.CurrentDirectory = currentTreeItem;
            }
        }

        /// <summary>
        /// Name of the current directory user is in.
        /// </summary>
        public DirInfo CurrentDirectory
        {
            get
            {
                return currentDirectory;
            }

            set
            {
                currentDirectory = value;
                this.RefreshCurrentItems();
                this.RaisePropertyChanged(nameof(this.CurrentDirectory));
            }
        }

        /// <summary>
        /// Children of the current directory to show in the right pane.
        /// </summary>
        public FullyObservableCollection<DirInfo> CurrentItems
        {
            get
            {
                if (currentItems == null)
                {
                    currentItems = new FullyObservableCollection<DirInfo>();
                }

                return currentItems;
            }
            set
            {
                currentItems = value;
                this.RaisePropertyChanged(nameof(this.CurrentItems));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curDir"></param>
        public void ExpandToCurrentNode(DirInfo curDir)
        {
            // Expand the current selected node in tree if this is an ancestor of the directory we want to navigate or "My Computer" current node 
            if (this.CurrentTreeItem != null && (curDir.Path.Contains(this.CurrentTreeItem.Path) || this.CurrentTreeItem.Path == "My computer"))
            {
                // Expand the current node If the current node is already expanded then first collapse it n then expand it
                this.CurrentTreeItem.IsExpanded = false;
                this.CurrentTreeItem.IsExpanded = true;
            }
        }

        /// <summary>
        /// This method gets the children of current directory and stores them in the CurrentItems Observable collection.
        /// </summary>
        protected void RefreshCurrentItems()
        {
            IList<DirInfo> childDirList = new List<DirInfo>();
            IList<DirInfo> childFileList = new List<DirInfo>();

            // Combine all the subdirectories and files of the current directory
            childDirList = FileSystemExplorerService.GetChildDirectories(this.CurrentDirectory.Path).Select(directory => new DirInfo(directory)).ToList();
            childFileList = FileSystemExplorerService.GetChildFiles(this.CurrentDirectory.Path).Select(file => new DirInfo(file)).ToList();
            childDirList = childDirList.Concat(childFileList).ToList();

            this.CurrentItems = new FullyObservableCollection<DirInfo>(childDirList);
        }

        /// <summary>
        /// Prompts the user to set a new project root.
        /// </summary>
        private void SetProjectRoot()
        {
            try
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.SelectedPath = SettingsViewModel.GetInstance().ProjectRoot;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !String.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                    {
                        if (Directory.Exists(folderBrowserDialog.SelectedPath))
                        {
                            SettingsViewModel.GetInstance().ProjectRoot = folderBrowserDialog.SelectedPath;
                        }
                    }
                    else
                    {
                        throw new Exception("Folder not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to open project", ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Prompts the user to open a project.
        /// </summary>
        private void OpenProject()
        {
            try
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.SelectedPath = SettingsViewModel.GetInstance().ProjectRoot;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !String.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                    {
                        if (Directory.Exists(folderBrowserDialog.SelectedPath))
                        {
                            DirInfo rootNode = new DirInfo(new DirectoryInfo(folderBrowserDialog.SelectedPath).Name);
                            rootNode.Path = folderBrowserDialog.SelectedPath;

                            this.SystemDirectorySource = new FullyObservableCollection<DirInfo> { rootNode };
                            this.CurrentDirectory = rootNode;
                        }
                        else
                        {
                            throw new Exception("Folder not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to open project", ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Prompts the user to create a new project.
        /// </summary>
        private void NewProject()
        {

        }
    }
    //// End class
}
//// End namespace