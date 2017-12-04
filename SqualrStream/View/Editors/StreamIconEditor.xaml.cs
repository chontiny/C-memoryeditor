namespace SqualrStream.View.Editors
{
    using SqualrStream.Source.Editors.StreamIconEditor;
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for StreamIconEditor.xaml.
    /// </summary>
    public partial class StreamIconEditor : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamIconEditor" /> class.
        /// </summary>
        /// <param name="path">The initial icon path.</param>
        public StreamIconEditor(String path = null)
        {
            this.InitializeComponent();

            this.StreamIconEditorViewModel.SelectionCallBack = this.TakeSelection;
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
            this.StreamIconEditorViewModel.SetIconCommand.Execute(this.StreamIconEditorViewModel.SelectedStreamIcon);
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
        private void TakeSelection()
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ListViewSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            this.StreamIconEditorViewModel.SelectIconCommand.Execute(e.AddedItems.Count > 0 ? e.AddedItems[0] : null);
        }
    }
    //// End class
}
//// End namespace