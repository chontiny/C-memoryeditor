namespace Squalr.Source.ProjectExplorer
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Properties;
    using Squalr.View.Dialogs;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Input;

    internal class OpenProjectDialogViewModel : ViewModelBase
    {
        private String searchTerm;

        private String selectedProject;

        /// <summary>
        /// Singleton instance of the <see cref="OpenProjectDialogViewModel" /> class.
        /// </summary>
        private static Lazy<OpenProjectDialogViewModel> openProjectDialogViewModelInstance = new Lazy<OpenProjectDialogViewModel>(
                () => { return new OpenProjectDialogViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        public OpenProjectDialogViewModel() : base()
        {
            this.UpdateSelectedProjectCommand = new RelayCommand<Object>((selectedItems) => this.SelectedProject = (selectedItems as IList)?.Cast<String>()?.FirstOrDefault());
        }

        public IEnumerable<String> Projects
        {
            get
            {
                return Directory.EnumerateDirectories(SettingsViewModel.GetInstance().ProjectRoot).Select(path => new DirectoryInfo(path).Name);
            }
        }

        public IEnumerable<String> FilteredProjects
        {
            get
            {
                return this.ProjectSearchTerm == null ? this.Projects : this.Projects.Select(project => project).Where(project => project.ToLower().Contains(this.ProjectSearchTerm.ToLower()));
            }
        }

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

        /// <summary>
        /// Gets a singleton instance of the <see cref="OpenProjectDialogViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static OpenProjectDialogViewModel GetInstance()
        {
            return OpenProjectDialogViewModel.openProjectDialogViewModelInstance.Value;
        }

        /// <summary>
        /// Gets the command to update the current project selection
        /// </summary>
        public ICommand UpdateSelectedProjectCommand { get; private set; }

        public void ShowDialog(Action<String> projectPathCallback)
        {
            OpenProjectDialog openProjectDialog = new OpenProjectDialog();

            if (openProjectDialog.ShowDialog() == true)
            {
                String projectPath = Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, this.SelectedProject);

                projectPathCallback?.Invoke(projectPath);
            }
        }
    }
    //// End class
}
//// End namespace