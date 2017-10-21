namespace SqualrCore.View
{
    using SqualrCore.Properties;
    using SqualrCore.Source.Editors.HotkeyEditor;
    using SqualrCore.Source.Editors.ScriptEditor;
    using SqualrCore.Source.Editors.TextEditor;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProcessSelector;
    using SqualrCore.Source.PropertyViewer;
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    public abstract class ViewTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
        }

        /// <summary>
        /// Gets or sets the template for the Process Selector.
        /// </summary>
        public DataTemplate ProcessSelectorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Property Viewer.
        /// </summary>
        public DataTemplate PropertyViewerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Settings.
        /// </summary>
        public DataTemplate SettingsViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Output.
        /// </summary>
        public DataTemplate OutputViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Offset Editor.
        /// </summary>
        public DataTemplate OffsetEditorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Script Editor.
        /// </summary>
        public DataTemplate ScriptEditorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Text Editor.
        /// </summary>
        public DataTemplate TextEditorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Hotkey Manager.
        /// </summary>
        public DataTemplate HotkeyManagerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Hotkey Editor.
        /// </summary>
        public DataTemplate HotkeyEditorViewTemplate { get; set; }

        /// <summary>
        /// Returns the required template to display the given view model.
        /// </summary>
        /// <param name="item">The view model.</param>
        /// <param name="container">The dependency object.</param>
        /// <returns>The template associated with the provided view model.</returns>
        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is ProcessSelectorViewModel)
            {
                return this.ProcessSelectorViewTemplate;
            }
            else if (item is PropertyViewerViewModel)
            {
                return this.PropertyViewerViewTemplate;
            }
            else if (item is SettingsViewModel)
            {
                return this.SettingsViewTemplate;
            }
            else if (item is OutputViewModel)
            {
                return this.OutputViewTemplate;
            }
            else if (item is ScriptEditorViewModel)
            {
                return this.ScriptEditorViewTemplate;
            }
            else if (item is TextEditorViewModel)
            {
                return this.TextEditorViewTemplate;
            }
            else if (item is HotkeyEditorViewModel)
            {
                return this.HotkeyEditorViewTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
    //// End class
}
//// End namespace