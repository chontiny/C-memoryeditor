namespace Squalr.Source.Editors.HotkeyEditor
{
    using Squalr.Engine.Input.HotKeys;
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows;

    /// <summary>
    /// Type editor for hot keys.
    /// </summary>
    public class HotkeyEditorModel : UITypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyEditorModel" /> class.
        /// </summary>
        public HotkeyEditorModel()
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
            Hotkey hotkey = value as Hotkey;
            View.Editors.HotkeyEditor hotkeyEditor = new View.Editors.HotkeyEditor(hotkey);
            hotkeyEditor.Owner = Application.Current.MainWindow;

            if (hotkeyEditor.ShowDialog() == true)
            {
                hotkey = hotkeyEditor.HotkeyEditorViewModel.ActiveHotkey.Build(hotkey);

                if (hotkey != null)
                {
                    return hotkey;
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