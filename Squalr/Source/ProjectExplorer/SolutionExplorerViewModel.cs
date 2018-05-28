namespace Squalr.Source.SolutionExplorer
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Microsoft.Win32;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Docking;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
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
            DirInfo rootNode = new DirInfo("My computer");
            rootNode.Path = "My computer";

            this.systemDirectorySource = new FullyObservableCollection<DirInfo> { rootNode };
            this.currentDirectory = rootNode;

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

            // If current directory is "My computer" then get the all logical drives in the system
            if (CurrentDirectory.Name.Equals("My computer"))
            {
                childDirList = (from rd in FileSystemExplorerService.GetRootDirectories()
                                select new DirInfo(rd)).ToList();
            }
            else
            {
                // Combine all the subdirectories and files of the current directory
                childDirList = FileSystemExplorerService.GetChildDirectories(CurrentDirectory.Path).Select(directory => new DirInfo(directory)).ToList();
                childFileList = FileSystemExplorerService.GetChildFiles(CurrentDirectory.Path).Select(file => new DirInfo(file)).ToList();
                childDirList = childDirList.Concat(childFileList).ToList();
            }

            CurrentItems = new FullyObservableCollection<DirInfo>(childDirList);
        }

        /// <summary>
        /// Prompts the user to open a project.
        /// </summary>
        private void OpenProject()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ProjectExtensionFilter;
            openFileDialog.Title = "Open Project";

            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            // Open the project file
            try
            {
                if (!File.Exists(openFileDialog.FileName))
                {
                    throw new Exception("File not found");
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