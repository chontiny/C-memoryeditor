namespace Squalr.View.Dialogs
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for SelectProjectDialog.xaml.
    /// </summary>
    public partial class SelectProjectDialog : Window
    {
        private String projectName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectProjectDialog" /> class.
        /// </summary>
        public SelectProjectDialog()
        {
            this.InitializeComponent();
        }

        public String ProjectName
        {
            get
            {
                return this.projectName;
            }

            set
            {
                this.projectName = value;
            }
        }

        public void SelectProject(String projectName)
        {
            this.ProjectName = projectName;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Invoked when the added offsets are canceled. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void CancelButtonClick(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Invoked when the added offsets are accepted. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void AcceptButtonClick(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ProjectsListViewMouseDoubleClick(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
    //// End class
}
//// End namespace