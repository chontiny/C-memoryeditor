namespace Squalr.Source.ProjectExplorer
{
    using Docking;
    using Engine;
    using Engine.OperatingSystems;
    using GalaSoft.MvvmLight.Command;
    using Main;
    using ProjectItems;
    using PropertyViewer;
    using Squalr.Properties;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Utils.Extensions;

    /// <summary>
    /// View model for the Project Explorer.
    /// </summary>
    internal class ProjectExplorerViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(ProjectExplorerViewModel);

        /// <summary>
        /// The filter to use for saving and loading project filters.
        /// </summary>
        public const String ProjectExtensionFilter = "Cheat File (*.Hax)|*.hax|All files (*.*)|*.*";

        /// <summary>
        /// The file extension for project items.
        /// </summary>
        private const String ProjectFileExtension = ".hax";

        /// <summary>
        /// The file extension for hotkeys.
        /// </summary>
        private const String HotkeyFileExtension = ".hotkeys";

        /// <summary>
        /// Singleton instance of the <see cref="ProjectExplorerViewModel" /> class.
        /// </summary>
        private static Lazy<ProjectExplorerViewModel> projectExplorerViewModelInstance = new Lazy<ProjectExplorerViewModel>(
                () => { return new ProjectExplorerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The project root which contains all project items.
        /// </summary>
        private ObservableCollection<ProjectItem> projectItems;

        /// <summary>
        /// The selected project item.
        /// </summary>
        private IEnumerable<ProjectItem> selectedProjectItems;

        /// <summary>
        /// Whether or not there are unsaved project changes.
        /// </summary>
        private Boolean hasUnsavedChanges;

        /// <summary>
        /// The file path to the project file.
        /// </summary>
        private String projectFilePath;

        /// <summary>
        /// Prevents a default instance of the <see cref="ProjectExplorerViewModel" /> class from being created.
        /// </summary>
        private ProjectExplorerViewModel() : base("Project Explorer")
        {
            this.ContentId = ProjectExplorerViewModel.ToolContentId;
            this.ObserverLock = new Object();
            this.ProjectItemStorage = new ProjectItemStorage();

            // Commands to manipulate project items may not be async due to multi-threading issues when modifying collections
            this.OpenProjectCommand = new RelayCommand(() => this.ProjectItemStorage.OpenProject(), () => true);
            this.ImportProjectCommand = new RelayCommand(() => this.ProjectItemStorage.ImportProject(), () => true);
            this.ExportProjectCommand = new RelayCommand(() => this.ProjectItemStorage.ExportProject(), () => true);
            this.ImportSpecificProjectCommand = new RelayCommand<String>((filename) => this.ProjectItemStorage.ImportProject(false, filename), (filename) => true);
            this.SaveProjectCommand = new RelayCommand(() => this.ProjectItemStorage.SaveProject(), () => true);
            this.SelectProjectItemCommand = new RelayCommand<ProjectItem>((projectItem) => PropertyViewerViewModel.GetInstance().SetTargetObjects(projectItem), (projectItem) => true);
            this.AddNewAddressItemCommand = new RelayCommand(() => this.AddNewPointerItem(), () => true);
            this.AddNewScriptItemCommand = new RelayCommand(() => this.AddNewScriptItem(), () => true);
            this.DeleteSelectionCommand = new RelayCommand(() => this.DeleteSelection(), () => true);
            this.ToggleSelectionActivationCommand = new RelayCommand(() => this.ToggleSelectionActivation(), () => true);
            this.ProjectItems = new ObservableCollection<ProjectItem>();
            this.Update();

            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
        }

        /// <summary>
        /// Gets the command to open a project from disk.
        /// </summary>
        public ICommand OpenProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to import another project from disk.
        /// </summary>
        public ICommand ImportProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to export a project to separate files.
        /// </summary>
        public ICommand ExportProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a specific project from disk, used for loading downloaded web projects.
        /// </summary>
        public ICommand ImportSpecificProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a project from disk.
        /// </summary>
        public ICommand SaveProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new address.
        /// </summary>
        public ICommand AddNewAddressItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new script.
        /// </summary>
        public ICommand AddNewScriptItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a project item.
        /// </summary>
        public ICommand SelectProjectItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete the selected project explorer items.
        /// </summary>
        public ICommand DeleteSelectionCommand { get; private set; }

        /// <summary>
        /// Gets the command to toggle the activation the selected project explorer items.
        /// </summary>
        public ICommand ToggleSelectionActivationCommand { get; private set; }

        /// <summary>
        /// Gets the command to clear the selected project item.
        /// </summary>
        public ICommand ClearSelectionCommand { get; private set; }

        /// <summary>
        /// Gets an object that allows accesses to saving and loading project items.
        /// </summary>
        public ProjectItemStorage ProjectItemStorage { get; private set; }

        /// <summary>
        /// Gets the root that contains all project items.
        /// </summary>
        public ObservableCollection<ProjectItem> ProjectItems
        {
            get
            {
                return this.projectItems;
            }

            set
            {
                this.projectItems = value;
                this.RaisePropertyChanged(nameof(this.ProjectItems));
            }
        }

        /// <summary>
        /// Gets project items that can be bound to a hotkey.
        /// </summary>
        public ObservableCollection<ProjectItem> BindableProjectItems
        {
            get
            {
                return new ObservableCollection<ProjectItem>(this.projectItems);
            }
        }

        /// <summary>
        /// Gets or sets the selected project items.
        /// </summary>
        public IEnumerable<ProjectItem> SelectedProjectItems
        {
            get
            {
                return this.selectedProjectItems;
            }

            set
            {
                this.selectedProjectItems = value;
                PropertyViewerViewModel.GetInstance().SetTargetObjects(this.SelectedProjectItems?.ToArray());
            }
        }

        /// <summary>
        /// Gets or sets a lock that controls access to observing classes.
        /// </summary>
        private Object ObserverLock { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ProjectExplorerViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ProjectExplorerViewModel GetInstance()
        {
            return ProjectExplorerViewModel.projectExplorerViewModelInstance.Value;
        }

        /// <summary>
        /// Disables all active project items.
        /// </summary>
        public void DisableAllProjectItems()
        {
            this.ProjectItems?.ForEach(item => item.IsActivated = false);
        }

        /// <summary>
        /// Adds a specific address to the project explorer.
        /// </summary>
        /// <param name="baseAddress">The address.</param>
        /// <param name="elementType">The value type.</param>
        public void AddSpecificAddressItem(IntPtr baseAddress, Type elementType)
        {
            // Check if the address is within a module, adding it as module format if so
            foreach (NormalizedModule module in EngineCore.GetInstance().OperatingSystem.GetModules())
            {
                if (module.ContainsAddress(baseAddress))
                {
                    this.AddNewProjectItems(
                        addToSelected: true,
                        projectItems: new PointerItem(
                            baseAddress: baseAddress.Subtract(module.BaseAddress),
                            elementType: elementType,
                            moduleName: module.Name));

                    return;
                }
            }

            this.AddNewProjectItems(
                addToSelected: true,
                projectItems: new PointerItem(
                    baseAddress: baseAddress,
                    elementType: elementType));
        }

        /// <summary>
        /// Adds the new project item to the project item collection.
        /// </summary>
        /// <param name="addToSelected">Whether or not the items should be added under the selected item.</param>
        /// <param name="projectItems">The project item to add.</param>
        public void AddNewProjectItems(Boolean addToSelected = true, params ProjectItem[] projectItems)
        {
            if (projectItems.IsNullOrEmpty())
            {
                return;
            }

            foreach (ProjectItem projectItem in projectItems)
            {
                this.ProjectItems.Add(projectItem);
            }

            this.RaisePropertyChanged(nameof(this.ProjectItems));
        }

        /// <summary>
        /// Called when a property is updated in any of the contained project items.
        /// </summary>
        public void OnPropertyUpdate()
        {
            this.RaisePropertyChanged(nameof(this.ProjectItems));
        }

        /// <summary>
        /// Adds a new address to the project items.
        /// </summary>
        private void AddNewPointerItem()
        {
            this.AddNewProjectItems(true, new PointerItem());
        }

        /// <summary>
        /// Adds a new script to the project items.
        /// </summary>
        private void AddNewScriptItem()
        {
            this.AddNewProjectItems(true, new ScriptItem());
        }

        /// <summary>
        /// Deletes the selected project explorer items.
        /// </summary>
        private void DeleteSelection()
        {
            foreach (ProjectItem projectItem in this.SelectedProjectItems)
            {
                this.ProjectItems.Remove(projectItem);
            }

            this.SelectedProjectItems = null;
        }

        /// <summary>
        /// Toggles the activation the selected project explorer items.
        /// </summary>
        private void ToggleSelectionActivation()
        {
            if (this.SelectedProjectItems == null || this.SelectedProjectItems.Count() < 0)
            {
                return;
            }

            foreach (ProjectItem projectItem in this.SelectedProjectItems)
            {
                if (projectItem != null)
                {
                    projectItem.IsActivated = !projectItem.IsActivated;
                }
            }
        }

        /// <summary>
        /// Continously updates the project explorer items.
        /// </summary>
        private void Update()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    foreach (ProjectItem projectItem in this.ProjectItems)
                    {
                        projectItem.Update();
                    }

                    Thread.Sleep(SettingsViewModel.GetInstance().TableReadInterval);
                }
            });
        }
    }
    //// End class
}
//// End namespace