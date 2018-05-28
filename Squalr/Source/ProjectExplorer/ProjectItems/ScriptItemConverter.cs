namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using Squalr.Source.Controls;
    using Squalr.Source.Editors.ScriptEditor;
    using Squalr.Source.Utils.TypeConverters;
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Runtime.Serialization;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class ScriptItemConverter : ScriptItem
    {

        /// <summary>
        /// Gets or sets the raw script text.
        /// </summary>
        [DataMember]
        [ReadOnly(false)]
        [TypeConverter(typeof(ScriptConverter))]
        [Editor(typeof(ScriptEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Script"), Description("C# script to interface with engine")]
        public override String Script
        {
            get
            {
                return base.Script;
            }

            set
            {
                base.Script = value;
            }
        }
    }
    //// End class
}
//// End namespace