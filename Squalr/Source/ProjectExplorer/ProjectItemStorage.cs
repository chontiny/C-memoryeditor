namespace Squalr.Source.ProjectExplorer
{
    using Microsoft.Win32;
    using Squalr.Engine.Output;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Properties;
    using Squalr.Source.Controls;
    using Squalr.Source.ProjectItems;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// View model for the Project Explorer.
    /// </summary>
    public class ProjectItemStorage
    {
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
        /// Creates an instance of the <see cref="ProjectItemStorage" /> class from being created.
        /// </summary>
        public ProjectItemStorage()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether there are unsaved changes.
        /// </summary>
        public Boolean HasUnsavedChanges { get; set; }

        /// <summary>
        /// Gets the path to the project file.
        /// </summary>
        public String ProjectFilePath { get; set; }

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
        /// Opens a project from disk.
        /// </summary>
        public void OpenProject()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ProjectExtensionFilter;
            openFileDialog.Title = "Open Project";

            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            ProjectExplorerViewModel.GetInstance().ProjectItems = new FullyObservableCollection<ProjectItem>();
            this.ProjectFilePath = openFileDialog.FileName;

            // Open the project file
            try
            {
                if (!File.Exists(this.ProjectFilePath))
                {
                    Output.Log(LogLevel.Error, "Unable to locate project.");
                    return;
                }

                using (FileStream fileStream = new FileStream(this.ProjectFilePath, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItem[]));
                    ProjectExplorerViewModel.GetInstance().ProjectItems = new FullyObservableCollection<ProjectItem>(serializer.ReadObject(fileStream) as ProjectItem[]);
                    this.HasUnsavedChanges = false;
                }
            }
            catch (Exception ex)
            {
                this.ProjectFilePath = String.Empty;
                Output.Log(LogLevel.Error, "Unable to open project", ex.ToString());
                return;
            }

            // Open the hotkey file
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(this.ProjectFilePath);

                if (File.Exists(hotkeyFilePath))
                {
                    using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Open, FileAccess.Read))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                        ProjectItemHotkey[] projectItemHotkeys = serializer.ReadObject(fileStream) as ProjectItemHotkey[];

                        this.BindHotkeys(ProjectExplorerViewModel.GetInstance().ProjectItems, projectItemHotkeys);
                    }
                }
            }
            catch (Exception ex)
            {
                Output.Log(LogLevel.Warn, "Unable to open hotkey profile", ex);
                return;
            }
        }

        /// <summary>
        /// Imports a project from disk, adding the project items to the current project.
        /// </summary>
        /// <param name="filename">The file path of the project to import.</param>
        public void ImportProject(Boolean resetGuids = true, String filename = null)
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
                    Output.Log(LogLevel.Error, "Unable to locate project.");
                    return;
                }

                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ProjectItem[]));
                    importedProjectRoot = deserializer.ReadObject(fileStream) as ProjectItem[];

                    // Add each high level child in the project root to this project
                    foreach (ProjectItem child in importedProjectRoot)
                    {
                        ProjectExplorerViewModel.GetInstance().AddNewProjectItems(false, importedProjectRoot);
                    }

                    this.HasUnsavedChanges = true;
                }
            }
            catch (Exception ex)
            {
                Output.Log(LogLevel.Error, "Unable to import project", ex);
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
                Output.Log(LogLevel.Warn, "Unable to open hotkey profile", ex);
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
        public void SaveProject()
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
                        serializer.WriteObject(fileStream, ProjectExplorerViewModel.GetInstance().ProjectItems.ToArray());
                    }

                    this.HasUnsavedChanges = false;
                }
            }
            catch (Exception ex)
            {
                Output.Log(LogLevel.Fatal, "Unable to save project", ex);
                return;
            }

            // Save the hotkey profile
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(this.ProjectFilePath);

                using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                    ProjectItemHotkey[] hotkeys = ProjectExplorerViewModel.GetInstance().ProjectItems?.Select(x => new ProjectItemHotkey(x.HotKey, x.Guid)).ToArray();
                    serializer.WriteObject(fileStream, hotkeys);
                }
            }
            catch (Exception ex)
            {
                Output.Log(LogLevel.Error, "Unable to save hotkey profile", ex);
                return;
            }
        }

        /// <summary>
        /// Export a project to separate files.
        /// </summary>
        public void ExportProject()
        {
            Task.Run(() =>
            {
                // Export the project items to thier own individual files
                try
                {
                    if (String.IsNullOrEmpty(this.ProjectFilePath) || !Directory.Exists(Path.GetDirectoryName(this.ProjectFilePath)))
                    {
                        Output.Log(LogLevel.Info, "Please save the project before exporting");
                        return;
                    }

                    Output.Log(LogLevel.Info, "Project export starting");

                    String folderPath = Path.Combine(Path.GetDirectoryName(this.ProjectFilePath), "Export");
                    Directory.CreateDirectory(folderPath);

                    Parallel.ForEach(
                        ProjectExplorerViewModel.GetInstance().ProjectItems,
                        SettingsViewModel.GetInstance().ParallelSettingsFast,
                        (projectItem) =>
                    {
                        ProjectItem targetProjectItem = projectItem;

                        if (targetProjectItem == null)
                        {
                            return;
                        }

                        String filePath = Path.Combine(folderPath, targetProjectItem.Name + ProjectItemStorage.ProjectFileExtension);

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
                    Output.Log(LogLevel.Fatal, "Unable to complete export project", ex);
                    return;
                }

                Output.Log(LogLevel.Info, "Project export complete");
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
            return Path.Combine(Path.GetDirectoryName(projectFilePath), Path.GetFileNameWithoutExtension(projectFilePath)) + ProjectItemStorage.HotkeyFileExtension;
        }
    }
    //// End class
}
//// End namespace