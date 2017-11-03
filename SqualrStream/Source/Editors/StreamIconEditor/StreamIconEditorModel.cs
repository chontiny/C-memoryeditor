namespace SqualrStream.Source.Editors.StreamIconEditor
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows;

    /// <summary>
    /// Stream icon path editor.
    /// </summary>
    public class StreamIconEditorModel : UITypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamIconEditorModel" /> class.
        /// </summary>
        public StreamIconEditorModel()
        {
        }

        /// <summary>
        /// Gets the editor style. This will be Modal, as it launches a custom editor.
        /// </summary>
        /// <param name="context">Type descriptor context.</param>
        /// <returns>Modal type editor.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// Launches the editor for this type.
        /// </summary>
        /// <param name="context">Type descriptor context.</param>
        /// <param name="provider">Service provider.</param>
        /// <param name="value">The current value.</param>
        /// <returns>The updated values.</returns>
        public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
        {
            View.Editors.StreamIconEditor streamIconEditor = new View.Editors.StreamIconEditor(value as String);

            streamIconEditor.Owner = Application.Current.MainWindow;
            if (streamIconEditor.ShowDialog() == true)
            {
                return streamIconEditor.StreamIconEditorViewModel.StreamIconName ?? String.Empty;
            }

            return value;
        }
    }
    //// End class
}
//// End namespace