namespace Squalr.View.Editors
{
    using Source.Controls;
    using Squalr.Source.Editors.ValueEditor;
    using Squalr.Source.ProjectItems;
    using System;
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

            this.ValueHexDecBox = new HexDecTextBox(addressItem.DataType);
            this.ValueHexDecBox.TextChanged += this.ValueHexDecBoxTextChanged;
            this.ValueHexDecBox.IsHex = addressItem.IsValueHex;
            this.ValueHexDecBox.SetValue(addressItem.AddressValue);
            this.ValueEditorTextEditorContainer.Children.Add(WinformsHostingHelper.CreateHostedControl(this.ValueHexDecBox));
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
        /// Gets or sets the hex dec box for the value.
        /// </summary>
        private HexDecTextBox ValueHexDecBox { get; set; }

        /// <summary>
        /// Text changed event for the value HexDec box.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void ValueHexDecBoxTextChanged(Object sender, EventArgs e)
        {
            Object value = this.ValueHexDecBox.GetValue();

            if (value == null)
            {
                return;
            }

            this.ValueEditorViewModel.Value = value;
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
            this.Close();
        }

        /// <summary>
        /// Event when all view content has been rendered.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void SqualrValueEditorContentRendered(Object sender, EventArgs e)
        {
            this.ValueHexDecBox.Focus();
            this.ValueHexDecBox.SelectAll();
        }
    }
    //// End class
}
//// End namespace