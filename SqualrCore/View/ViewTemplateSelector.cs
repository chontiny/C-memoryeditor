namespace SqualrCore.View
{
    using SqualrCore.Source.Editors.HotkeyEditor;
    using SqualrCore.Source.Editors.ScriptEditor;
    using SqualrCore.Source.Editors.TextEditor;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProcessSelector;
    using SqualrCore.Source.PropertyViewer;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    public abstract class ViewTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// The template for the Process Selector.
        /// </summary>
        private DataTemplate processSelectorViewTemplate;

        /// <summary>
        /// The template for the Property Viewer.
        /// </summary>
        private DataTemplate propertyViewerViewTemplate;

        /// <summary>
        /// The template for the Output.
        /// </summary>
        private DataTemplate outputViewTemplate;

        /// <summary>
        /// The template for the Offset Editor.
        /// </summary>
        private DataTemplate offsetEditorViewTemplate;

        /// <summary>
        /// The template for the Script Editor.
        /// </summary>
        private DataTemplate scriptEditorViewTemplate;

        /// <summary>
        /// The template for the Text Editor.
        /// </summary>
        private DataTemplate textEditorViewTemplate;

        /// <summary>
        /// The template for the Hotkey Editor.
        /// </summary>
        private DataTemplate hotkeyEditorViewTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
            this.DataTemplates = new Dictionary<Type, DataTemplate>();
        }

        /// <summary>
        /// Gets or sets the template for the Process Selector.
        /// </summary>
        public DataTemplate ProcessSelectorViewTemplate
        {
            get
            {
                return this.processSelectorViewTemplate;
            }

            set
            {
                this.processSelectorViewTemplate = value;
                this.DataTemplates[typeof(ProcessSelectorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Property Viewer.
        /// </summary>
        public DataTemplate PropertyViewerViewTemplate
        {
            get
            {
                return this.propertyViewerViewTemplate;
            }

            set
            {
                this.propertyViewerViewTemplate = value;
                this.DataTemplates[typeof(PropertyViewerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Output.
        /// </summary>
        public DataTemplate OutputViewTemplate
        {
            get
            {
                return this.outputViewTemplate;
            }

            set
            {
                this.outputViewTemplate = value;
                this.DataTemplates[typeof(OutputViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Offset Editor.
        /// </summary>
        public DataTemplate OffsetEditorViewTemplate
        {
            get
            {
                return this.offsetEditorViewTemplate;
            }

            set
            {
                this.offsetEditorViewTemplate = value;
                this.DataTemplates[typeof(OffsetEditorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Script Editor.
        /// </summary>
        public DataTemplate ScriptEditorViewTemplate
        {
            get
            {
                return this.scriptEditorViewTemplate;
            }

            set
            {
                this.scriptEditorViewTemplate = value;
                this.DataTemplates[typeof(ScriptEditorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Text Editor.
        /// </summary>
        public DataTemplate TextEditorViewTemplate
        {
            get
            {
                return this.textEditorViewTemplate;
            }

            set
            {
                this.textEditorViewTemplate = value;
                this.DataTemplates[typeof(TextEditorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Hotkey Editor.
        /// </summary>
        public DataTemplate HotkeyEditorViewTemplate
        {
            get
            {
                return this.hotkeyEditorViewTemplate;
            }

            set
            {
                this.hotkeyEditorViewTemplate = value;
                this.DataTemplates[typeof(HotkeyEditorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Data Template Error display.
        /// </summary>
        public DataTemplate DataTemplateErrorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the mapping for all data templates.
        /// </summary>
        protected Dictionary<Type, DataTemplate> DataTemplates { get; set; }

        /// <summary>
        /// Returns the required template to display the given view model.
        /// </summary>
        /// <param name="item">The view model.</param>
        /// <param name="container">The dependency object.</param>
        /// <returns>The template associated with the provided view model.</returns>
        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (this.DataTemplates.ContainsKey(item.GetType()))
            {
                return this.DataTemplates[item.GetType()];
            }

            return this.DataTemplateErrorViewTemplate;
        }
    }
    //// End class
}
//// End namespace