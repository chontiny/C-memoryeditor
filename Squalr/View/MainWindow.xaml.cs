namespace Squalr.View
{
    using Squalr.Source.Main;
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    internal partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public MainViewModel MainViewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        /// <summary>
        /// Event fired when the main window is rendered.
        /// </summary>
        /// <param name="Sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void SqualrWindowContentRendered(Object Sender, EventArgs e)
        {
            this.MainViewModel.DisplayChangeLogCommand?.Execute(null);
        }

        /// <summary>
        /// Event fired when the main window is loaded.
        /// </summary>
        /// <param name="Sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void SqualrWindowLoaded(Object sender, RoutedEventArgs e)
        {
            this.MainViewModel.LoadLayoutCommand?.Execute(this.dockManager);
        }

        /// <summary>
        /// Event fired when the main window is unloaded.
        /// </summary>
        /// <param name="Sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void SqualrWindowUnloaded(Object sender, RoutedEventArgs e)
        {
            this.MainViewModel.SaveLayoutCommand?.Execute(this.dockManager);
        }
    }
    //// End class
}
//// End namespace