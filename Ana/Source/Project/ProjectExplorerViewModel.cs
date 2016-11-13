namespace Ana.Source.Project
{
    using Docking;
    using Main;
    using Microsoft.Win32;
    using Mvvm.Command;
    using ProjectItems;
    using PropertyViewer;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using UserSettings;
    /// <summary>
    /// View model for the Project Explorer
    /// </summary>
    internal class ProjectExplorerViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model
        /// </summary>
        public const String ToolContentId = nameof(ProjectExplorerViewModel);

        /// <summary>
        /// The filter to use for saving and loading project filters
        /// </summary>
        public const String ProjectExtensionFilter = "Cheat File (*.Hax)|*.hax|All files (*.*)|*.*";

        /// <summary>
        /// Singleton instance of the <see cref="ProjectExplorerViewModel" /> class
        /// </summary>
        private static Lazy<ProjectExplorerViewModel> projectExplorerViewModelInstance = new Lazy<ProjectExplorerViewModel>(
                () => { return new ProjectExplorerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The project root which contains all project items
        /// </summary>
        private ProjectItemViewModel projectRoot;

        /// <summary>
        /// The selected project item
        /// </summary>
        private ProjectItemViewModel selectedProjectItem;

        /// <summary>
        /// Whether or not there are unsaved project changes
        /// </summary>
        private Boolean hasUnsavedChanges;

        /// <summary>
        /// The file path to the project file
        /// </summary>
        private String projectFilePath;

        /// <summary>
        /// Prevents a default instance of the <see cref="ProjectExplorerViewModel" /> class from being created
        /// </summary>
        private ProjectExplorerViewModel() : base("Project Explorer")
        {
            this.ContentId = ProjectExplorerViewModel.ToolContentId;

            // Commands to manipulate project items may not be async due to multi-threading issues when modifying collections
            this.OpenProjectCommand = new RelayCommand(() => this.OpenProject(), () => true);
            this.ImportProjectCommand = new RelayCommand(() => this.ImportProject(), () => true);
            this.ImportSpecificProjectCommand = new RelayCommand<String>((filename) => this.ImportProject(filename), (filename) => true);
            this.SaveProjectCommand = new RelayCommand(() => this.SaveProject(), () => true);
            this.SaveAsProjectCommand = new RelayCommand(() => this.SaveAsProject(), () => true);
            this.AddNewFolderItemCommand = new RelayCommand(() => this.AddNewFolderItem(), () => true);
            this.AddNewAddressItemCommand = new RelayCommand(() => this.AddNewAddressItem(), () => true);
            this.AddNewScriptItemCommand = new RelayCommand(() => this.AddNewScriptItem(), () => true);
            this.DeleteSelectionCommand = new RelayCommand(() => this.DeleteSelection(), () => true);
            this.ToggleSelectionActivationCommand = new RelayCommand(() => this.ToggleSelectionActivation(), () => true);
            this.projectRoot = new ProjectItemViewModel(new ProjectRoot());
            this.IsVisible = true;
            this.Update();

            Task.Run(() => MainViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets the command to open a project from disk
        /// </summary>
        public ICommand OpenProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a project from disk
        /// </summary>
        public ICommand ImportProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a specific project from disk, used for loading downloaded web projects
        /// </summary>
        public ICommand ImportSpecificProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a project from disk
        /// </summary>
        public ICommand SaveProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a project from disk
        /// </summary>
        public ICommand SaveAsProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new folder
        /// </summary>
        public ICommand AddNewFolderItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new address
        /// </summary>
        public ICommand AddNewAddressItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new script
        /// </summary>
        public ICommand AddNewScriptItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete the selected project explorer items
        /// </summary>
        public ICommand DeleteSelectionCommand { get; private set; }

        /// <summary>
        /// Gets the command to toggle the activation the selected project explorer items
        /// </summary>
        public ICommand ToggleSelectionActivationCommand { get; private set; }

        /// <summary>
        /// Gets the command to clear the selected project item
        /// </summary>
        public ICommand ClearSelectionCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether there are unsaved changes
        /// </summary>
        public Boolean HasUnsavedChanges
        {
            get
            {
                return this.hasUnsavedChanges;
            }

            set
            {
                this.hasUnsavedChanges = value;
                this.RaisePropertyChanged(nameof(this.HasUnsavedChanges));
            }
        }

        /// <summary>
        /// Gets the path to the project file
        /// </summary>
        public String ProjectFilePath
        {
            get
            {
                return this.projectFilePath;
            }

            private set
            {
                this.projectFilePath = value;
                this.RaisePropertyChanged(nameof(this.ProjectFilePath));
            }
        }

        /// <summary>
        /// The root that contains all project items
        /// </summary>
        public ProjectItemViewModel ProjectRoot
        {
            get
            {
                return this.projectRoot;
            }

            private set
            {
                this.projectRoot = value;
                this.RaisePropertyChanged(nameof(this.ProjectRoot));
            }
        }

        /// <summary>
        /// Gets or sets the selected project item
        /// </summary>
        public ProjectItemViewModel SelectedProjectItem
        {
            get
            {
                return this.selectedProjectItem;
            }

            set
            {
                this.selectedProjectItem = value;
                PropertyViewerViewModel.GetInstance().SetTargetObjects(this.selectedProjectItem?.ProjectItem);
                this.RaisePropertyChanged(nameof(this.SelectedProjectItem));
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ProjectExplorerViewModel" /> class
        /// </summary>
        /// <returns>A singleton instance of the class</returns>
        public static ProjectExplorerViewModel GetInstance()
        {
            return ProjectExplorerViewModel.projectExplorerViewModelInstance.Value;
        }

        /// <summary>
        /// Adds a specific address to the project explorer
        /// </summary>
        /// <param name="baseAddress">The address</param>
        /// <param name="elementType">The value type</param>
        public void AddSpecificAddressItem(IntPtr baseAddress, Type elementType)
        {
            this.AddNewProjectItem(new AddressItem(baseAddress, elementType));
        }

        /// <summary>
        /// Adds the new project item to the project item collection
        /// </summary>
        /// <param name="projectItem">The project item to add</param>
        public void AddNewProjectItem(ProjectItem projectItem)
        {
            ProjectItemViewModel folderTarget = this.SelectedProjectItem;

            // Atempt to find the correct folder to place the new item into
            while (folderTarget != null && !(folderTarget.ProjectItem is FolderItem))
            {
                folderTarget = folderTarget.Parent as ProjectItemViewModel;
            }

            if (folderTarget != null)
            {
                folderTarget.AddChild(new ProjectItemViewModel(projectItem, folderTarget));
            }
            else
            {
                this.projectRoot.AddChild(new ProjectItemViewModel(projectItem, this.projectRoot));
            }
        }

        /// <summary>
        /// Adds a new folder to the project items
        /// </summary>
        private void AddNewFolderItem()
        {
            this.AddNewProjectItem(new FolderItem());
        }

        /// <summary>
        /// Adds a new address to the project items
        /// </summary>
        private void AddNewAddressItem()
        {
            this.AddNewProjectItem(new AddressItem());
        }

        /// <summary>
        /// Adds a new script to the project items
        /// </summary>
        private void AddNewScriptItem()
        {
            this.AddNewProjectItem(new ScriptItem());
        }

        /// <summary>
        /// Deletes the selected project explorer items
        /// </summary>
        private void DeleteSelection()
        {
            this.ProjectRoot.RemoveChildRecursive(this.SelectedProjectItem);
            this.SelectedProjectItem = null;
        }

        /// <summary>
        /// Toggles the activation the selected project explorer items
        /// </summary>
        private void ToggleSelectionActivation()
        {
            ProjectItem projectItem = this.SelectedProjectItem?.ProjectItem;
            if (projectItem != null)
            {
                projectItem.IsActivated = !projectItem.IsActivated;
            }
        }

        /// <summary>
        /// Continously updates the project explorer items
        /// </summary>
        private void Update()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    this.projectRoot.ProjectItem.Update();
                    Thread.Sleep(SettingsViewModel.GetInstance().TableReadInterval);
                }
            });
        }

        /// <summary>
        /// Opens a project from disk
        /// </summary>
        private void OpenProject()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ProjectExtensionFilter;
            openFileDialog.Title = "Open Project";
            openFileDialog.ShowDialog();
            this.ProjectFilePath = openFileDialog.FileName;

            if (this.ProjectFilePath == null || this.ProjectFilePath == String.Empty)
            {
                return;
            }

            try
            {
                using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                    this.ProjectRoot = new ProjectItemViewModel(serializer.ReadObject(fileStream) as ProjectRoot);
                    this.ProjectRoot.BuildViewModels();
                    this.HasUnsavedChanges = false;
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Imports a project from disk, adding the project items to the current project
        /// </summary>
        private void ImportProject(String filename = null)
        {
            // Ask for a specific file if one was not explicitly provided
            if (filename == null || filename == String.Empty)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = ProjectExtensionFilter;
                openFileDialog.Title = "Import Project";
                openFileDialog.ShowDialog();
                this.ProjectFilePath = openFileDialog.FileName;

                if (this.ProjectFilePath == null || this.ProjectFilePath == String.Empty)
                {
                    return;
                }
            }

            try
            {
                using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                    ProjectRoot importedProjectRoot = serializer.ReadObject(fileStream) as ProjectRoot;
                    importedProjectRoot.Children.ForEach(x => (this.ProjectRoot.ProjectItem as ProjectRoot).AddChild(x));
                    this.ProjectRoot.BuildViewModels();
                    this.HasUnsavedChanges = true;
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Save a project to disk
        /// </summary>
        private void SaveProject()
        {
            if (this.ProjectFilePath == null || this.ProjectFilePath == String.Empty)
            {
                this.SaveAsProject();
                return;
            }

            try
            {
                using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                    serializer.WriteObject(fileStream, this.projectRoot.ProjectItem);
                }

                this.HasUnsavedChanges = false;
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Save a project to disk under a new specified project name
        /// </summary>
        private void SaveAsProject()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ProjectExplorerViewModel.ProjectExtensionFilter;
            saveFileDialog.Title = "Save Project";
            saveFileDialog.ShowDialog();
            this.ProjectFilePath = saveFileDialog.FileName;
            this.SaveProject();
        }
    }
    //// End class
}
//// End namespace