namespace Squalr.Source.SolutionExplorer
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Docking;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows.Input;

    public class SolutionExplorerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="ProjectExplorerViewModel" /> class.
        /// </summary>
        private static Lazy<SolutionExplorerViewModel> solutionExplorerViewModelInstance = new Lazy<SolutionExplorerViewModel>(
                () => { return new SolutionExplorerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);


        private DirInfo currentTreeItem;
        private FullyObservableCollection<DirInfo> systemDirectorySource;

        private DirInfo currentDirectory;
        private FullyObservableCollection<DirInfo> currentItems;
        private Boolean showDirectoryTree = true;
        private ICommand showTreeCommand;

        public SolutionExplorerViewModel() : base("Solution Explorer")
        {
            DirInfo rootNode = new DirInfo("My computer");
            rootNode.Path = "My computer";
            this.systemDirectorySource = new FullyObservableCollection<DirInfo> { rootNode };
            this.currentDirectory = rootNode;

            this.ShowTreeCommand = new RelayCommand(() => this.DirectoryTreeHideHandler());

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
        /// list of the directories 
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
        /// Current selected item in the tree
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
        /// Name of the current directory user is in
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
        /// Visibility of the 
        /// </summary>
        public Boolean ShowDirectoryTree
        {
            get
            {
                return showDirectoryTree;
            }
            set
            {
                showDirectoryTree = value;
                this.RaisePropertyChanged(nameof(this.ShowDirectoryTree));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ShowTreeCommand
        {
            get
            {
                return showTreeCommand;
            }

            set
            {
                showTreeCommand = value;
                this.RaisePropertyChanged(nameof(this.ShowTreeCommand));
            }
        }

        /// <summary>
        /// Children of the current directory to show in the right pane
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

        private void DirectoryTreeHideHandler()
        {
            this.ShowDirectoryTree = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curDir"></param>
        public void ExpandToCurrentNode(DirInfo curDir)
        {
            //expand the current selected node in tree 
            //if this is an ancestor of the directory we want to navigate or "My Computer" current node 
            if (this.CurrentTreeItem != null && (curDir.Path.Contains(this.CurrentTreeItem.Path) || this.CurrentTreeItem.Path == "My computer"))
            {
                // expand the current node
                // If the current node is already expanded then first collapse it n then expand it
                this.CurrentTreeItem.IsExpanded = false;
                this.CurrentTreeItem.IsExpanded = true;
            }
        }

        /// <summary>
        /// this method gets the children of current directory and stores them in the CurrentItems Observable collection
        /// </summary>
        protected void RefreshCurrentItems()
        {
            return;
            IList<DirInfo> childDirList = new List<DirInfo>();
            IList<DirInfo> childFileList = new List<DirInfo>();

            //If current directory is "My computer" then get the all logical drives in the system
            if (CurrentDirectory.Name.Equals("My computer"))
            {
                childDirList = (from rd in FileSystemExplorerService.GetRootDirectories()
                                select new DirInfo(rd)).ToList();
            }
            else
            {
                //Combine all the subdirectories and files of the current directory
                childDirList = (from dir in FileSystemExplorerService.GetChildDirectories(CurrentDirectory.Path)
                                select new DirInfo(dir)).ToList();

                childFileList = (from fobj in FileSystemExplorerService.GetChildFiles(CurrentDirectory.Path)
                                 select new DirInfo(fobj)).ToList();

                childDirList = childDirList.Concat(childFileList).ToList();
            }

            CurrentItems = new FullyObservableCollection<DirInfo>(childDirList);
        }
    }
    //// End class
}
//// End namespace