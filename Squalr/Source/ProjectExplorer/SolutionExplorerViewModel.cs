namespace Squalr.Source.SolutionExplorer
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Projects;
    using Squalr.Engine.Utils;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Properties;
    using Squalr.Source.Docking;
    using Squalr.Source.Editors.ScriptEditor;
    using Squalr.Source.Editors.ValueEditor;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using Squalr.Source.PropertyViewer;
    using System;
    using System.IO;
    using System.Linq;
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

        private FullyObservableCollection<DirectoryItemView> projectRoot;

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
            this.EditProjectItemCommand = new RelayCommand<ProjectItem>((projectItem) => this.EditProjectItem(projectItem), (projectItem) => true);
            this.AddNewAddressItemCommand = new RelayCommand(() => this.AddNewProjectItem(typeof(PointerItem)), () => true);
            this.AddNewScriptItemCommand = new RelayCommand(() => this.AddNewProjectItem(typeof(ScriptItem)), () => true);
            this.AddNewInstructionItemCommand = new RelayCommand(() => this.AddNewProjectItem(typeof(InstructionItem)), () => true);

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
        /// Gets the command to add a new address.
        /// </summary>
        public ICommand AddNewAddressItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new instruction.
        /// </summary>
        public ICommand AddNewInstructionItemCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a new script.
        /// </summary>
        public ICommand AddNewScriptItemCommand { get; private set; }

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
        /// The project root tree of the current project.
        /// </summary>
        public FullyObservableCollection<DirectoryItemView> ProjectRoot
        {
            get
            {
                return projectRoot;
            }
            set
            {
                projectRoot = value;
                RaisePropertyChanged(nameof(this.ProjectRoot));
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
                            this.ProjectRoot = new FullyObservableCollection<DirectoryItemView> { new DirectoryItemView(new DirectoryItem(folderBrowserDialog.SelectedPath)) };
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

        /// <summary>
        /// Adds a new address to the project items.
        /// </summary>
        private void AddNewProjectItem(Type projectItemType)
        {
            switch (projectItemType)
            {
                case Type _ when projectItemType == typeof(PointerItem):
                    this.ProjectRoot.FirstOrDefault()?.AddChild(new PointerItem());
                    break;
                case Type _ when projectItemType == typeof(ScriptItem):
                    this.ProjectRoot.FirstOrDefault()?.AddChild(new ScriptItem());
                    break;
                case Type _ when projectItemType == typeof(InstructionItem):
                    this.ProjectRoot.FirstOrDefault()?.AddChild(new InstructionItem());
                    break;
                default:
                    Logger.Log(LogLevel.Error, "Unknown project item type - " + projectItemType.ToString());
                    break;
            }
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

                if (SyntaxChecker.CanParseValue(addressItem.DataType, result?.ToString()))
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
            /*
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
            */
        }

        /// <summary>
        /// Deletes the selected project explorer items.
        /// </summary>
        private void DeleteSelection(Boolean promptUser = true)
        {
            /*
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
            */
        }

        /// <summary>
        /// Copies the selected project explorer items.
        /// </summary>
        private void CopySelection()
        {
            // this.ClipBoard = this.SelectedProjectItems?.SoftClone();
        }

        /// <summary>
        /// Pastes the copied project explorer items.
        /// </summary>
        private void PasteSelection()
        {
            // if (this.ClipBoard == null || this.ClipBoard.Count() <= 0)
            // {
            //     return;
            // }

            // ProjectExplorerViewModel.GetInstance().AddNewProjectItems(true, this.ClipBoard);
        }

        /// <summary>
        /// Cuts the selected project explorer items.
        /// </summary>
        private void CutSelection()
        {
            //  this.ClipBoard = this.SelectedProjectItems?.SoftClone();
            // this.DeleteSelection(promptUser: false);
        }
    }
    //// End class
}
//// End namespace