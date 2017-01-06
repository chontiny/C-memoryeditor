namespace Ana.View
{
    using Controls;
    using Source.Project.ProjectItems;
    using Source.Utils.ValueEditor;
    using System;
    using System.Windows;
    /// <summary>
    /// Interaction logic for ValueEditor.xaml
    /// </summary>
    internal partial class ValueEditor : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptEditor" /> class
        /// </summary>
        /// <param name="value">The initial value text being edited</param>
        public ValueEditor(AddressItem addressItem)
        {
            this.InitializeComponent();

            this.ValueHexDecBox = new HexDecTextBox(addressItem.ElementType);
            this.ValueHexDecBox.TextChanged += ValueHexDecBoxTextChanged;
            this.ValueHexDecBox.IsHex = addressItem.IsValueHex;
            this.ValueHexDecBox.SetValue(addressItem.Value);
            this.ValueEditorTextEditorContainer.Children.Add(WinformsHostingHelper.CreateHostedControl(this.ValueHexDecBox));
        }

        private void ValueHexDecBoxTextChanged(Object sender, EventArgs e)
        {
            dynamic value = this.ValueHexDecBox.GetValue();

            if (value == null)
            {
                return;
            }

            this.ValueEditorViewModel.Value = value;
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
        /// Gets or sets the hex dec box for the value
        /// </summary>
        private HexDecTextBox ValueHexDecBox { get; set; }

        /// <summary>
        /// Invoked when the added offsets are canceled. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void CancelButtonClick(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Invoked when the added offsets are accepted. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void AcceptButtonClick(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Invoked when the exit file menu event executes. Closes the view.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event args</param>
        private void ExitFileMenuItemClick(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AnaValueEditorContentRendered(Object sender, EventArgs e)
        {
            this.ValueHexDecBox.Focus();
            this.ValueHexDecBox.SelectAll();
        }
    }
    //// End class
}
//// End namespace