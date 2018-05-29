namespace Squalr.Source.SolutionExplorer
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Projects;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Properties;
    using Squalr.Source.Docking;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using Squalr.Source.PropertyViewer;
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;

    internal class SolutionExplorerViewModel : ToolViewModel
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

        private FullyObservableCollection<ProjectItemView> projectItems;

        /// <summary>
        /// The selected project item.
        /// </summary>
        private ProjectItemView selectedProjectItem;

        public SolutionExplorerViewModel() : base("Solution Explorer")
        {
            this.SetProjectRootCommand = new RelayCommand(() => this.SetProjectRoot());
            this.OpenProjectCommand = new RelayCommand(() => this.OpenProject());
            this.NewProjectCommand = new RelayCommand(() => this.NewProject());
            this.SelectProjectItemCommand = new RelayCommand<Object>((selectedItem) => this.SelectedProjectItem = selectedItem as ProjectItemView, (selectedItem) => true);

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
        /// Gets the command to select a project item.
        /// </summary>
        public ICommand SelectProjectItemCommand { get; private set; }

        /// <summary>
        /// List of the directories.
        /// </summary>
        public FullyObservableCollection<ProjectItemView> ProjectItems
        {
            get
            {
                return projectItems;
            }
            set
            {
                projectItems = value;
                RaisePropertyChanged(nameof(this.ProjectItems));
            }
        }

        /// <summary>
        /// Gets or sets the selected project items.
        /// </summary>
        public ProjectItemView SelectedProjectItem
        {
            get
            {
                return this.selectedProjectItem;
            }

            set
            {
                this.selectedProjectItem = value;
                PropertyViewerViewModel.GetInstance().SetTargetObjects(value);
            }
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
                            this.ProjectItems = new FullyObservableCollection<ProjectItemView> { new DirectoryItemView(new DirectoryItem(folderBrowserDialog.SelectedPath)) };
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