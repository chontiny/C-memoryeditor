namespace Squalr.Source.ProjectExplorer
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Utils.Extensions;
    using Squalr.Properties;
    using Squalr.View.Dialogs;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    internal class SelectProjectDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Singleton instance of the <see cref="SelectProjectDialogViewModel" /> class.
        /// </summary>
        private static Lazy<SelectProjectDialogViewModel> selectProjectDialogViewModelInstance = new Lazy<SelectProjectDialogViewModel>(
                () => { return new SelectProjectDialogViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private String searchTerm;

        private String selectedProject;

        private String newProjectName;

        private SelectProjectDialogViewModel() : base()
        {
            this.UpdateSelectedProjectCommand = new RelayCommand<Object>((selectedItems) => this.SelectedProject = (selectedItems as IList)?.Cast<String>()?.FirstOrDefault());
            this.OpenSelectedProjectCommand = new RelayCommand<String>((selectedProject) => this.OpenSelectedProject(selectedProject));
            this.RenameSelectedProjectCommand = new RelayCommand<String>((selectedProject) => this.RenameSelectedProject(selectedProject));
            this.NewProjectCommand = new RelayCommand(() => this.CreateNewProject());
            this.DeleteProjectCommand = new RelayCommand(() => this.DeleteProject());
        }

        /// <summary>
        /// Gets the command to update the current project selection.
        /// </summary>
        public ICommand UpdateSelectedProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the selected project.
        /// </summary>
        public ICommand OpenSelectedProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to rename the selected project.
        /// </summary>
        public ICommand RenameSelectedProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to create a new project.
        /// </summary>
        public ICommand NewProjectCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete a project.
        /// </summary>
        public ICommand DeleteProjectCommand { get; private set; }

        /// <summary>
        /// Gets a list of projects in the project root.
        /// </summary>
        public List<String> Projects
        {
            get
            {
                return Directory.EnumerateDirectories(SettingsViewModel.GetInstance().ProjectRoot).Select(path => new DirectoryInfo(path).Name).ToList();
            }
        }

        /// <summary>
        /// Gets a list of projects in the project root, filtered by the current search term.
        /// </summary>
        public List<String> FilteredProjects
        {
            get
            {
                return this.ProjectSearchTerm == null ? this.Projects : this.Projects.Select(project => project).Where(project => project.ToLower().Contains(this.ProjectSearchTerm.ToLower())).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the project search term to filter project results.
        /// </summary>
        public String ProjectSearchTerm
        {
            get
            {
                return this.searchTerm;
            }

            set
            {
                this.searchTerm = value;
                this.RaisePropertyChanged(nameof(this.ProjectSearchTerm));
                this.RaisePropertyChanged(nameof(this.FilteredProjects));
            }
        }

        /// <summary>
        /// Gets or sets the current selected project.
        /// </summary>
        public String SelectedProject
        {
            get
            {
                return this.selectedProject;
            }

            set
            {
                this.selectedProject = value;
                this.RaisePropertyChanged(nameof(this.SelectedProject));
            }
        }

        public String NewProjectName
        {
            get
            {
                return this.newProjectName;
            }

            set
            {
                this.newProjectName = value;
                this.RaisePropertyChanged(nameof(this.NewProjectName));
                this.RaisePropertyChanged(nameof(this.IsProjectNameValid));
                this.RaisePropertyChanged(nameof(this.NewProjectNameStatus));
            }
        }

        public Boolean IsProjectNameValid
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this.NewProjectName) || this.NewProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                    Directory.Exists(Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, this.NewProjectName)))
                {
                    return false;
                }

                return true;
            }
        }

        public String NewProjectNameStatus
        {
            get
            {
                if (this.NewProjectName != null)
                {
                    if (this.NewProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        return "Invalid project name";
                    }
                    else if (Directory.Exists(Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, this.NewProjectName)) && !String.IsNullOrWhiteSpace(this.NewProjectName))
                    {
                        return "Project already exists";
                    }
                }

                return String.Empty;
            }
        }

        private SelectProjectDialog SelectProjectDialog { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="SelectProjectDialogViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static SelectProjectDialogViewModel GetInstance()
        {
            return SelectProjectDialogViewModel.selectProjectDialogViewModelInstance.Value;
        }

        public void ShowDialog(Window owner, Action<String> projectPathCallback)
        {
            this.SelectProjectDialog = new SelectProjectDialog() { Owner = owner };

            if (this.SelectProjectDialog.ShowDialog() == true)
            {
                String projectPath = Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, this.SelectedProject);

                projectPathCallback?.Invoke(projectPath);
            }
        }

        private void OpenSelectedProject(String selectedProject)
        {
            //// ProjectExplorerViewModel.GetInstance().OpenProjectCommand()
        }

        private void RenameSelectedProject(String selectedProject)
        {
            RenameProjectDialogViewModel renameProjectDialog = RenameProjectDialogViewModel.GetInstance();

            if (renameProjectDialog.ShowDialog(this.SelectProjectDialog, selectedProject) == true)
            {
                this.RaisePropertyChanged(nameof(this.Projects));
                this.RaisePropertyChanged(nameof(this.FilteredProjects));
            }
        }

        private void CreateNewProject()
        {
            try
            {
                String newProjectDirectory = String.Empty;
                IEnumerable<String> projects = this.Projects;

                for (Int32 appendedNumber = 0; appendedNumber < Int32.MaxValue; appendedNumber++)
                {
                    String suffix = (appendedNumber == 0 ? String.Empty : " " + appendedNumber.ToString());
                    newProjectDirectory = Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, "New Project" + suffix);

                    if (!Directory.Exists(newProjectDirectory))
                    {
                        Directory.CreateDirectory(newProjectDirectory);
                        this.RaisePropertyChanged(nameof(this.Projects));
                        this.RaisePropertyChanged(nameof(this.FilteredProjects));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error creating new project directory", ex);
            }
        }

        private void DeleteProject()
        {
            if (this.SelectedProject.IsNullOrEmpty())
            {
                Logger.Log(LogLevel.Warn, "No project was selected to delete.");
                return;
            }

            String projectPath = Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, this.SelectedProject);

            if (!Directory.Exists(projectPath))
            {
                Logger.Log(LogLevel.Error, "Project does not exist on disk.");
                return;
            }

            if (DeleteProjectDialogViewModel.GetInstance().ShowDialog(this.SelectProjectDialog, this.SelectedProject))
            {
                this.RaisePropertyChanged(nameof(this.Projects));
                this.RaisePropertyChanged(nameof(this.FilteredProjects));
            }
        }
    }
    //// End class
}
//// End namespace