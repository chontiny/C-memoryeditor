namespace Ana.Source.Utils.OffsetEditor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Type editor for pointer offsets.
    /// </summary>
    internal class OffsetEditorModel : UITypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OffsetEditorModel" /> class.
        /// </summary>
        public OffsetEditorModel()
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
            View.OffsetEditor offsetEditor = new View.OffsetEditor(value == null ? null : (value as IEnumerable<Int32>)?.ToList());

            offsetEditor.Owner = Application.Current.MainWindow;
            if (offsetEditor.ShowDialog() == true)
            {
                List<Int32> newOffsets = offsetEditor.OffsetEditorViewModel.Offsets.ToList();

                if (newOffsets != null && newOffsets.Count > 0)
                {
                    return offsetEditor.OffsetEditorViewModel.Offsets.ToList();
                }
                else
                {
                    return null;
                }
            }

            return value;
        }
    }
    //// End class
}
//// End namespace