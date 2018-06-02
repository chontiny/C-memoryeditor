namespace Squalr.Source.ProjectExplorer
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Properties;
    using Squalr.View.Dialogs;
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows.Input;

    internal class NewProjectDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Singleton instance of the <see cref="NewProjectDialogViewModel" /> class.
        /// </summary>
        private static Lazy<NewProjectDialogViewModel> newProjectDialogViewModelInstance = new Lazy<NewProjectDialogViewModel>(
                () => { return new NewProjectDialogViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private String newProjectName;

        public NewProjectDialogViewModel() : base()
        {
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="NewProjectDialogViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static NewProjectDialogViewModel GetInstance()
        {
            return NewProjectDialogViewModel.newProjectDialogViewModelInstance.Value;
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

        public void ShowDialog(Action<String> newProjectPathCallback)
        {
            NewProjectDialog newProjectDialog = new NewProjectDialog();

            if (newProjectDialog.ShowDialog() == true && this.IsProjectNameValid)
            {
                String newProjectPath = Path.Combine(SettingsViewModel.GetInstance().ProjectRoot, this.NewProjectName);

                newProjectPathCallback?.Invoke(newProjectPath);
            }
        }

        private void SetProjectRoot()
        {

        }
    }
    //// End class
}
//// End namespace