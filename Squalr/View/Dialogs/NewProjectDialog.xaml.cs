namespace Squalr.View.Dialogs
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for NewProjectDialog.xaml.
    /// </summary>
    public partial class NewProjectDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewProjectDialog" /> class.
        /// </summary>
        public NewProjectDialog()
        {
            this.InitializeComponent();
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
    }
    //// End class
}
//// End namespace