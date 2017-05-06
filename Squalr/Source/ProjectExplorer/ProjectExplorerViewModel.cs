namespace Squalr.Source.ProjectExplorer
{
    using Controls;
    using Docking;
    using Engine;
    using Engine.AddressResolver;
    using Engine.OperatingSystems;
    using Main;
    using Microsoft.Win32;
    using Mvvm.Command;
    using Output;
    using ProjectItems;
    using PropertyViewer;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using UserSettings;
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
        private ProjectRoot projectRoot;

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
            this.ProjectExplorerObservers = new HashSet<IProjectExplorerObserver>();

            // Commands to manipulate project items may not be async due to multi-threading issues when modifying collections
            this.OpenProjectCommand = new RelayCommand(() => this.OpenProject(), () => true);
            this.ImportProjectCommand = new RelayCommand(() => this.ImportProject(), () => true);
            this.ExportProjectCommand = new RelayCommand(() => this.ExportProject(), () => true);
            this.ImportSpecificProjectCommand = new RelayCommand<String>((filename) => this.ImportProject(filename), (filename) => true);
            this.SaveProjectCommand = new RelayCommand(() => this.SaveProject(), () => true);
            this.SaveAsProjectCommand = new RelayCommand(() => this.SaveAsProject(), () => true);
            this.AddNewFolderItemCommand = new RelayCommand(() => this.AddNewFolderItem(), () => true);
            this.AddNewAddressItemCommand = new RelayCommand(() => this.AddNewAddressItem(), () => true);
            this.AddNewScriptItemCommand = new RelayCommand(() => this.AddNewScriptItem(), () => true);
            this.DeleteSelectionCommand = new RelayCommand(() => this.DeleteSelection(), () => true);
            this.ToggleSelectionActivationCommand = new RelayCommand(() => this.ToggleSelectionActivation(), () => true);
            this.ProjectRoot = new ProjectRoot();
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
        /// Gets the command to open a project from disk.
        /// </summary>
        public ICommand SaveAsProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new folder.
        /// </summary>
        public ICommand AddNewFolderItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new address.
        /// </summary>
        public ICommand AddNewAddressItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new script.
        /// </summary>
        public ICommand AddNewScriptItemCommand { get; private set; }

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
        /// Gets or sets a value indicating whether there are unsaved changes.
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
        /// Gets the path to the project file.
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
        /// Gets the root that contains all project items.
        /// </summary>
        public ProjectRoot ProjectRoot
        {
            get
            {
                return this.projectRoot;
            }

            private set
            {
                this.projectRoot?.RemoveAllNodes();
                this.projectRoot = value;
                this.NotifyObserversStructureChange();
                this.RaisePropertyChanged(nameof(this.ProjectRoot));
            }
        }

        /// <summary>
        /// Gets project items that can be bound to a hotkey.
        /// </summary>
        public ObservableCollection<ProjectItem> BindableProjectItems
        {
            get
            {
                return new ObservableCollection<ProjectItem>(this.projectRoot.Children);
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
        /// Gets or sets objects observing changes in the selected objects.
        /// </summary>
        private HashSet<IProjectExplorerObserver> ProjectExplorerObservers { get; set; }

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
            this.ProjectRoot.Flatten().ForEach(item => item.IsActivated = false);
        }

        /// <summary>
        /// Disables all active project items with stream commands.
        /// </summary>
        public void DisableAllStreamProjectItems()
        {
            this.ProjectRoot.Flatten().Select(item => item).Where(item => !String.IsNullOrWhiteSpace(item.StreamCommand)).ForEach(item => item.IsActivated = false);
        }

        /// <summary>
        /// Prompts the user to save the project if there are unsaved changes.
        /// </summary>
        /// <returns>Returns false if canceled, otherwise true.</returns>
        public Boolean PromptSave()
        {
            if (!this.HasUnsavedChanges)
            {
                return true;
            }

            String projectName = Path.GetFileName(this.ProjectFilePath);

            if (String.IsNullOrWhiteSpace(projectName))
            {
                projectName = "Untitled";
            }

            MessageBoxResult result = CenteredDialogBox.Show(
                "Save changes to project " + projectName + "?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
            {
                return false;
            }

            if (result == MessageBoxResult.Yes)
            {
                this.SaveProject();
            }

            return true;
        }

        public void ActivateStreamCommands(IEnumerable<String> streamCommands)
        {

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
                        projectItems: new AddressItem(
                            baseAddress: baseAddress.Subtract(module.BaseAddress),
                            elementType: elementType,
                            resolveType: AddressResolver.ResolveTypeEnum.Module,
                            baseIdentifier: module.Name));

                    return;
                }
            }

            this.AddNewProjectItems(
                addToSelected: true,
                projectItems: new AddressItem(
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
                if (ProjectRoot.HasNode(projectItem))
                {
                    return;
                }

                ProjectItem target = this.SelectedProjectItems?.FirstOrDefault();

                // Atempt to find the correct folder to place the new item into
                while (target != null && !(target is FolderItem))
                {
                    target = target.Parent as ProjectItem;
                }

                FolderItem targetFolder = target as FolderItem;

                if (target != null)
                {
                    targetFolder.AddChild(projectItem);
                }
                else
                {
                    this.ProjectRoot.AddChild(projectItem);
                }
            }

            this.NotifyObserversStructureChange();
        }

        /// <summary>
        /// Called when a property is updated in any of the contained project items.
        /// </summary>
        public void OnPropertyUpdate()
        {
            this.NotifyObserversValueChange();
        }

        /// <summary>
        /// Subscribes the given object to changes in the project structure.
        /// </summary>
        /// <param name="projectExplorerObserver">The object to observe project structure changes.</param>
        public void Subscribe(IProjectExplorerObserver projectExplorerObserver)
        {
            lock (this.ObserverLock)
            {
                if (!this.ProjectExplorerObservers.Contains(projectExplorerObserver))
                {
                    this.ProjectExplorerObservers.Add(projectExplorerObserver);
                    projectExplorerObserver.Update(this.ProjectRoot);
                    projectExplorerObserver.UpdateStructure(this.ProjectRoot);
                }
            }
        }

        /// <summary>
        /// Unsubscribes the given object from changes in the project structure.
        /// </summary>
        /// <param name="projectExplorerObserver">The object to observe project structure changes.</param>
        public void Unsubscribe(IProjectExplorerObserver projectExplorerObserver)
        {
            lock (this.ObserverLock)
            {
                if (this.ProjectExplorerObservers.Contains(projectExplorerObserver))
                {
                    this.ProjectExplorerObservers.Remove(projectExplorerObserver);
                }
            }
        }

        /// <summary>
        /// Adds a new folder to the project items.
        /// </summary>
        private void AddNewFolderItem()
        {
            this.AddNewProjectItems(true, new FolderItem());
        }

        /// <summary>
        /// Adds a new address to the project items.
        /// </summary>
        private void AddNewAddressItem()
        {
            this.AddNewProjectItems(true, new AddressItem());
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
            this.ProjectRoot.RemoveNodes(this.SelectedProjectItems);
            this.SelectedProjectItems = null;

            this.NotifyObserversStructureChange();
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
                    this.ProjectRoot.Update();
                    this.NotifyObserversValueChange();
                    Thread.Sleep(SettingsViewModel.GetInstance().TableReadInterval);
                }
            });
        }

        /// <summary>
        /// Opens a project from disk.
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

            this.ProjectFilePath = openFileDialog.FileName;

            // Open the project file
            try
            {
                if (!File.Exists(this.ProjectFilePath))
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to locate project.");
                    return;
                }

                using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                    this.ProjectRoot = serializer.ReadObject(fileStream) as ProjectRoot;
                    this.HasUnsavedChanges = false;
                }
            }
            catch (Exception ex)
            {
                this.ProjectFilePath = String.Empty;
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to open project", ex.ToString());
                return;
            }

            // Open the hotkey file
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(this.projectFilePath);

                if (File.Exists(hotkeyFilePath))
                {
                    using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Open, FileAccess.Read))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                        ProjectItemHotkey[] projectItemHotkeys = serializer.ReadObject(fileStream) as ProjectItemHotkey[];

                        this.BindHotkeys(this.ProjectRoot.Flatten(), projectItemHotkeys);
                    }
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to open hotkey profile", ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Imports a project from disk, adding the project items to the current project.
        /// </summary>
        /// <param name="filename">The file path of the project to import.</param>
        private void ImportProject(String filename = null)
        {
            // Ask for a specific file if one was not explicitly provided
            if (filename == null || filename == String.Empty)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = ProjectExtensionFilter;
                openFileDialog.Title = "Import Project";

                if (openFileDialog.ShowDialog() == false)
                {
                    return;
                }

                filename = openFileDialog.FileName;

                // Clear the current project, such that on save the user is prompted to reselect this
                this.ProjectFilePath = null;
            }

            // Import the project file
            ProjectRoot importedProjectRoot = null;
            try
            {
                if (!File.Exists(filename))
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to locate project.");
                    return;
                }

                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                    importedProjectRoot = serializer.ReadObject(fileStream) as ProjectRoot;

                    // Add each high level child in the project root to this project
                    foreach (ProjectItem child in importedProjectRoot.Children)
                    {
                        this.AddNewProjectItems(false, child);
                    }

                    this.HasUnsavedChanges = true;
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to import project", ex.ToString());
                return;
            }

            // Import the hotkey file
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(filename);

                if (File.Exists(hotkeyFilePath))
                {
                    using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Open, FileAccess.Read))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                        ProjectItemHotkey[] projectItemHotkeys = serializer.ReadObject(fileStream) as ProjectItemHotkey[];

                        // Bind the hotkey to this project item
                        this.BindHotkeys(importedProjectRoot?.Flatten(), projectItemHotkeys);
                    }
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to open hotkey profile", ex.ToString());
                return;
            }

            // Randomize the guid for imported project items, preventing possible conflicts
            if (importedProjectRoot != null)
            {
                foreach (ProjectItem child in importedProjectRoot.Flatten())
                {
                    child.ResetGuid();
                }
            }
        }

        /// <summary>
        /// Save a project to disk.
        /// </summary>
        private void SaveProject()
        {
            // Save the project file
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.ProjectFilePath)))
                {
                    this.SaveAsProject();
                    return;
                }

                using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                    serializer.WriteObject(fileStream, this.ProjectRoot);
                }

                this.HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Unable to save project", ex.ToString());
                return;
            }

            // Save the hotkey profile
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(this.projectFilePath);

                using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                    ProjectItemHotkey[] hotkeys = ProjectExplorerViewModel.GetInstance().ProjectRoot?.Flatten().Select(x => new ProjectItemHotkey(x.HotKey, x.StreamCommand, x.Guid)).ToArray();
                    serializer.WriteObject(fileStream, hotkeys);
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to save hotkey profile", ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Export a project to separate files.
        /// </summary>
        private void ExportProject()
        {
            // Export the project items to thier own individual files
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "Select a Folder to Export Project Items";
                saveFileDialog.Title = "Export Project";

                if (saveFileDialog.ShowDialog() == true)
                {
                    String folderPath = Path.GetDirectoryName(saveFileDialog.FileName);

                    Parallel.ForEach(
                        this.ProjectRoot.Flatten().Where(x => !(x is FolderItem)),
                        SettingsViewModel.GetInstance().ParallelSettingsFast,
                        (projectItem) =>
                    {
                        ProjectItem targetProjectItem = projectItem;

                        if (projectItem is ScriptItem)
                        {
                            ScriptItem scriptItem = projectItem as ScriptItem;

                            try
                            {
                                if (!scriptItem.IsCompiled)
                                {
                                    targetProjectItem = scriptItem?.Compile();
                                }
                            }
                            catch (Exception ex)
                            {
                                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to compile a project item - " + targetProjectItem?.Description, ex.ToString());
                                return;
                            }
                        }

                        String filePath = Path.Combine(folderPath, targetProjectItem.Description + ProjectExplorerViewModel.ProjectFileExtension);

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            ProjectRoot newProjectRoot = new ProjectRoot();
                            newProjectRoot.AddChild(targetProjectItem);

                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectRoot));
                            serializer.WriteObject(fileStream, newProjectRoot);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Unable to complete export project", ex.ToString());
                return;
            }

            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Project export complete.");

        }

        /// <summary>
        /// Save a project to disk under a new specified project name.
        /// </summary>
        private void SaveAsProject()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ProjectExplorerViewModel.ProjectExtensionFilter;
            saveFileDialog.Title = "Save Project";

            if (saveFileDialog.ShowDialog() == true)
            {
                this.ProjectFilePath = saveFileDialog.FileName;
                this.SaveProject();
            }
        }

        /// <summary>
        /// Binds deserailized project item hotkeys to the corresponding project items.
        /// </summary>
        /// <param name="projectItems">The candidate project items to which we are binding the hotkeys.</param>
        /// <param name="projectItemHotkeys">The deserialized project item hotkeys.</param>
        private void BindHotkeys(IEnumerable<ProjectItem> projectItems, IEnumerable<ProjectItemHotkey> projectItemHotkeys)
        {
            if (projectItemHotkeys.IsNullOrEmpty())
            {
                return;
            }

            projectItemHotkeys
                .Join(
                    projectItems,
                    binding => binding.ProjectItemGuid,
                    item => item.Guid,
                    (binding, item) => new { binding = binding, item = item })
                .ForEach(x => x.item.LoadHotkey(x.binding.Hotkey))
                .ForEach(x => x.item.LoadStreamCommand(x.binding.StreamCommand));
        }

        /// <summary>
        /// Notify all observing objects of a change in the project structure.
        /// </summary>
        private void NotifyObserversStructureChange()
        {
            lock (this.ObserverLock)
            {
                foreach (IProjectExplorerObserver observer in this.ProjectExplorerObservers)
                {
                    observer.UpdateStructure(this.ProjectRoot);
                }
            }

            this.RaisePropertyChanged(nameof(this.ProjectRoot));
        }

        /// <summary>
        /// Notify all observing objects of a change in the project structure.
        /// </summary>
        private void NotifyObserversValueChange()
        {
            lock (this.ObserverLock)
            {
                foreach (IProjectExplorerObserver observer in this.ProjectExplorerObservers)
                {
                    observer.Update(this.ProjectRoot);
                }
            }
        }

        /// <summary>
        /// Gets the hotkey file path that corresponds to this project file path.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <returns>The file path to the hotkey file.</returns>
        private String GetHotkeyFilePathFromProjectFilePath(String projectFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(projectFilePath), Path.GetFileNameWithoutExtension(projectFilePath)) + ProjectExplorerViewModel.HotkeyFileExtension;
        }
    }
    //// End class
}
//// End namespace