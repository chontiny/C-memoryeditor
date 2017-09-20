namespace Squalr.Source.ProjectExplorer
{
    using Controls;
    using Docking;
    using Engine;
    using Engine.OperatingSystems;
    using GalaSoft.MvvmLight.Command;
    using Main;
    using Microsoft.Win32;
    using Output;
    using ProjectItems;
    using PropertyViewer;
    using Squalr.Properties;
    using Squalr.Source.Analytics;
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

            // Commands to manipulate project items may not be async due to multi-threading issues when modifying collections
            this.OpenProjectCommand = new RelayCommand(() => this.OpenProject(), () => true);
            this.ImportProjectCommand = new RelayCommand(() => this.ImportProject(), () => true);
            this.ExportProjectCommand = new RelayCommand(() => this.ExportProject(), () => true);
            this.ImportSpecificProjectCommand = new RelayCommand<String>((filename) => this.ImportProject(false, filename), (filename) => true);
            this.SaveProjectCommand = new RelayCommand(() => this.SaveProject(), () => true);
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
        public ObservableCollection<ProjectItem> ProjectItems
        {
            get
            {
                return this.projectItems;
            }

            private set
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

            this.ProjectItems = new ObservableCollection<ProjectItem>();
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
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItem[]));
                    this.ProjectItems = new ObservableCollection<ProjectItem>(serializer.ReadObject(fileStream) as ProjectItem[]);
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

                        this.BindHotkeys(this.ProjectItems, projectItemHotkeys);
                    }
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to open hotkey profile", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
                return;
            }
        }

        /// <summary>
        /// Imports a project from disk, adding the project items to the current project.
        /// </summary>
        /// <param name="filename">The file path of the project to import.</param>
        private void ImportProject(Boolean resetGuids = true, String filename = null)
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
            ProjectItem[] importedProjectRoot = null;

            try
            {
                if (!File.Exists(filename))
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to locate project.");
                    return;
                }

                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ProjectItem[]));
                    importedProjectRoot = deserializer.ReadObject(fileStream) as ProjectItem[];

                    // Add each high level child in the project root to this project
                    foreach (ProjectItem child in importedProjectRoot)
                    {
                        this.AddNewProjectItems(false, child);
                    }

                    this.HasUnsavedChanges = true;
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to import project", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
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
                        this.BindHotkeys(importedProjectRoot, projectItemHotkeys);
                    }
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to open hotkey profile", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
                return;
            }

            // Randomize the guid for imported project items, preventing possible conflicts
            if (resetGuids && importedProjectRoot != null)
            {
                foreach (ProjectItem child in importedProjectRoot)
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

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = ProjectExplorerViewModel.ProjectExtensionFilter;
                saveFileDialog.Title = "Save Project";
                saveFileDialog.FileName = String.IsNullOrWhiteSpace(this.ProjectFilePath) ? String.Empty : Path.GetFileName(this.ProjectFilePath);
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = String.IsNullOrWhiteSpace(this.ProjectFilePath) ? String.Empty : Path.GetDirectoryName(this.ProjectFilePath);

                if (saveFileDialog.ShowDialog() == true)
                {
                    this.ProjectFilePath = saveFileDialog.FileName;

                    using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Create, FileAccess.Write))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItem[]));
                        serializer.WriteObject(fileStream, this.ProjectItems);
                    }

                    this.HasUnsavedChanges = false;
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Unable to save project", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
                return;
            }

            // Save the hotkey profile
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(this.projectFilePath);

                using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                    ProjectItemHotkey[] hotkeys = ProjectExplorerViewModel.GetInstance().ProjectItems?.Select(x => new ProjectItemHotkey(x.HotKey, x.Guid)).ToArray();
                    serializer.WriteObject(fileStream, hotkeys);
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to save hotkey profile", ex);
                AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
                return;
            }
        }

        /// <summary>
        /// Export a project to separate files.
        /// </summary>
        private void ExportProject()
        {
            Task.Run(() =>
            {
                // Export the project items to thier own individual files
                try
                {
                    if (String.IsNullOrEmpty(this.ProjectFilePath) || !Directory.Exists(Path.GetDirectoryName(this.ProjectFilePath)))
                    {
                        OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Please save the project before exporting");
                        return;
                    }

                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Project export starting");

                    String folderPath = Path.Combine(Path.GetDirectoryName(this.ProjectFilePath), "Export");
                    Directory.CreateDirectory(folderPath);

                    Parallel.ForEach(
                        this.ProjectItems,
                        SettingsViewModel.GetInstance().ParallelSettingsFast,
                        (projectItem) =>
                    {
                        ProjectItem targetProjectItem = projectItem;

                        if (projectItem is ScriptItem)
                        {
                            ScriptItem scriptItem = projectItem as ScriptItem;

                            if (!scriptItem.IsCompiled)
                            {
                                targetProjectItem = scriptItem?.Compile();
                            }
                        }

                        if (targetProjectItem == null)
                        {
                            return;
                        }

                        String filePath = Path.Combine(folderPath, targetProjectItem.Description + ProjectExplorerViewModel.ProjectFileExtension);

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            List<ProjectItem> newProjectRoot = new List<ProjectItem>();
                            newProjectRoot.Add(targetProjectItem);

                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItem[]));
                            serializer.WriteObject(fileStream, newProjectRoot.ToArray());
                        }
                    });
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Unable to complete export project", ex);
                    AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, ex);
                    return;
                }

                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Project export complete");
            });
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
                .ForEach(x => x.item.LoadHotkey(x.binding.Hotkey));
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