namespace Squalr.View
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for StreamTable.xaml.
    /// </summary>
    internal partial class StreamTable : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamIconEditor" /> class.
        /// </summary>
        /// <param name="path">The initial icon path.</param>
        public StreamTable()
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
    }
    //// End class
}
//// End namespace