namespace Squalr.Source.ProjectExplorer
{
    using Docking;
    using GalaSoft.MvvmLight.Command;
    using Main;
    using ProjectItems;
    using PropertyViewer;
    using Squalr.Properties;
    using Squalr.Source.Controls;
    using Squalr.Source.Editors.ScriptEditor;
    using Squalr.Source.Editors.ValueEditor;
    using Squalr.Source.Utils;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.OperatingSystems;
    using System;
    using System.Collections;
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
            this.SelectProjectItemCommand = new RelayCommand<Object>((selectedItems) => this.SelectedProjectItems = (selectedItems as IList)?.Cast<ProjectItem>(), (selectedItems) => true);
            this.EditProjectItemCommand = new RelayCommand<ProjectItem>((projectItem) => this.EditProjectItem(projectItem), (projectItem) => true);
            this.AddNewAddressItemCommand = new RelayCommand(() => this.AddNewPointerItem(), () => true);
            this.AddNewScriptItemCommand = new RelayCommand(() => this.AddNewScriptItem(), () => true);
            this.ToggleSelectionActivationCommand = new RelayCommand(() => this.ToggleSelectionActivation(), () => true);
            this.DeleteSelectionCommand = new RelayCommand(() => this.DeleteSelection(), () => true);
            this.CopySelectionCommand = new RelayCommand(() => this.CopySelection(), () => true);
            this.PasteSelectionCommand = new RelayCommand(() => this.PasteSelection(), () => true);
            this.CutSelectionCommand = new RelayCommand(() => this.CutSelection(), () => true);
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
        /// Gets the command to edit a project item.
        /// </summary>
        public ICommand EditProjectItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to toggle the activation the selected project explorer items.
        /// </summary>
        public ICommand ToggleSelectionActivationCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete the selected project explorer items.
        /// </summary>
        public ICommand DeleteSelectionCommand { get; private set; }

        /// <summary>
        /// Gets the command to copy the selection to the clipboard.
        /// </summary>
        public ICommand CopySelectionCommand { get; private set; }

        /// <summary>
        /// Gets the command to paste the selection to the clipboard.
        /// </summary>
        public ICommand PasteSelectionCommand { get; private set; }

        /// <summary>
        /// Gets the command to cut the selection to the clipboard.
        /// </summary>
        public ICommand CutSelectionCommand { get; private set; }

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
        /// The current project items copied to the clip board.
        /// </summary>
        private IEnumerable<ProjectItem> ClipBoard { get; set; }

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
        /// Edits a project item based on the project item type.
        /// </summary>
        /// <param name="projectItem">The project item to edit.</param>
        private void EditProjectItem(ProjectItem projectItem)
        {
            if (projectItem is AddressItem)
            {
                ValueEditorModel valueEditor = new ValueEditorModel();
                AddressItem addressItem = projectItem as AddressItem;
                dynamic result = valueEditor.EditValue(null, null, addressItem);

                if (CheckSyntax.CanParseValue(addressItem.DataType, result?.ToString()))
                {
                    addressItem.AddressValue = result;
                }
            }
            else if (projectItem is ScriptItem)
            {
                ScriptEditorModel scriptEditor = new ScriptEditorModel();
                ScriptItem scriptItem = projectItem as ScriptItem;
                scriptItem.Script = scriptEditor.EditValue(null, null, scriptItem.Script) as String;
            }
        }

        /// <summary>
        /// Toggles the activation the selected project explorer items.
        /// </summary>
        private void ToggleSelectionActivation()
        {
            if (this.SelectedProjectItems == null)
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
        /// Deletes the selected project explorer items.
        /// </summary>
        private void DeleteSelection(Boolean promptUser = true)
        {
            if (this.SelectedProjectItems.IsNullOrEmpty())
            {
                return;
            }

            if (promptUser)
            {
                System.Windows.MessageBoxResult result = CenteredDialogBox.Show(
                    System.Windows.Application.Current.MainWindow,
                    "Delete selected items?",
                    "Confirm",
                    System.Windows.MessageBoxButton.OKCancel,
                    System.Windows.MessageBoxImage.Warning);

                if (result != System.Windows.MessageBoxResult.OK)
                {
                    return;
                }
            }

            foreach (ProjectItem projectItem in this.SelectedProjectItems.ToArray())
            {
                this.ProjectItems.Remove(projectItem);
            }

            this.SelectedProjectItems = null;
        }

        /// <summary>
        /// Copies the selected project explorer items.
        /// </summary>
        private void CopySelection()
        {
            this.ClipBoard = this.SelectedProjectItems?.SoftClone();
        }

        /// <summary>
        /// Pastes the copied project explorer items.
        /// </summary>
        private void PasteSelection()
        {
            if (this.ClipBoard == null || this.ClipBoard.Count() <= 0)
            {
                return;
            }

            foreach (ProjectItem projectItem in this.ClipBoard)
            {
                // We must clone the item, such as to prevent duplicate references of the same exact object
                ProjectExplorerViewModel.GetInstance().AddNewProjectItems(true, projectItem.Clone());
            }
        }

        /// <summary>
        /// Cuts the selected project explorer items.
        /// </summary>
        private void CutSelection()
        {
            this.ClipBoard = this.SelectedProjectItems?.SoftClone();
            this.DeleteSelection(promptUser: false);
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
                    foreach (ProjectItem projectItem in this.ProjectItems.ToArray())
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