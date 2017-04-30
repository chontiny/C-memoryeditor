namespace Ana.View.Editors
{
    using Ana.Source.Editors.StreamIconEditor;
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for StreamIconEditor.xaml.
    /// </summary>
    internal partial class StreamIconEditor : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamIconEditor" /> class.
        /// </summary>
        /// <param name="path">The initial icon path.</param>
        public StreamIconEditor(String path = null)
        {
            this.InitializeComponent();

            this.StreamIconEditorViewModel.SelectionCallBack = this.OnSelect;
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public StreamIconEditorViewModel StreamIconEditorViewModel
        {
            get
            {
                return this.DataContext as StreamIconEditorViewModel;
            }
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

        /// <summary>
        /// Invoked when the exit file menu event executes. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void ExitFileMenuItemClick(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Callback function for when a selection event is triggered.
        /// </summary>
        private void OnSelect()
        {
            this.DialogResult = true;
            this.Close();
        }
    }
    //// End class
}
//// End namespace