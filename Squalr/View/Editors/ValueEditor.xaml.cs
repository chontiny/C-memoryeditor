namespace Squalr.View.Editors
{
    using Source.Controls;
    using Squalr.Engine.Projects.Items;
    using Squalr.Source.Editors.ValueEditor;
    using System;
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for ValueEditor.xaml.
    /// </summary>
    public partial class ValueEditor : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueEditor" /> class.
        /// </summary>
        /// <param name="addressItem">The initial address being edited.</param>
        public ValueEditor(AddressItem addressItem)
        {
            this.InitializeComponent();

            this.ValueHexDecBoxViewModel = this.ValueHexDecBox.DataContext as HexDecBoxViewModel;
            this.ValueHexDecBoxViewModel.PropertyChanged += HexDecBoxViewModelPropertyChanged;
            this.ValueHexDecBoxViewModel.DataType = addressItem.DataType;
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public ValueEditorViewModel ValueEditorViewModel
        {
            get
            {
                return this.DataContext as ValueEditorViewModel;
            }
        }

        /// <summary>
        /// Gets or sets the view model for the hex dec box for the value.
        /// </summary>
        private HexDecBoxViewModel ValueHexDecBoxViewModel { get; set; }

        private void HexDecBoxViewModelPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(HexDecBoxViewModel.Text))
            {
                this.ValueEditorViewModel.Value = this.ValueHexDecBoxViewModel.GetValue();
            }
        }

        /// <summary>
        /// Invoked when the added offsets are canceled. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="args">Event args.</param>
        private void CancelButtonClick(Object sender, RoutedEventArgs args)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Invoked when the added offsets are accepted. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="args">Event args.</param>
        private void AcceptButtonClick(Object sender, RoutedEventArgs args)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Invoked when the exit file menu event executes. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="args">Event args.</param>
        private void ExitFileMenuItemClick(Object sender, RoutedEventArgs args)
        {
            this.Close();
        }

        /// <summary>
        /// Event when this window has been loaded.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="args">Event args.</param>
        private void SqualrValueEditorLoaded(Object sender, RoutedEventArgs args)
        {
            this.ValueHexDecBox.InnerTextBox.Focus();
        }
    }
    //// End class
}
//// End namespace