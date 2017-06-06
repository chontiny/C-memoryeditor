namespace Squalr.Source.Controls.TreeView
{
    using Aga.Controls.Tree;
    using ProjectExplorer.ProjectItems;
    using System;
    using System.Drawing;

    /// <summary>
    /// Defines a <see cref="TreeViewAdv"/> node in the project explorer.
    /// </summary>
    internal class ProjectNode : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectNode" /> class.
        /// </summary>
        /// <param name="entryDescription">String with which to set the description property.</param>
        public ProjectNode() : base(String.Empty)
        {
            this.EntryDescription = String.Empty;
            this.EntryStreamCommand = String.Empty;
            this.EntryHotkey = String.Empty;
            this.EntryValuePreview = String.Empty;
            this.EntryIcon = null;
        }

        /// <summary>
        /// Gets or sets the description of the project node.
        /// </summary>
        public String EntryDescription { get; set; }

        /// <summary>
        /// Gets or sets the stream command preview of the project node.
        /// </summary>
        public String EntryStreamCommand { get; set; }

        /// <summary>
        /// Gets or sets the hotkey preview of the project node.
        /// </summary>
        public String EntryHotkey { get; set; }

        /// <summary>
        /// Gets or sets the value preview of the project node.
        /// </summary>
        public String EntryValuePreview { get; set; }

        /// <summary>
        /// Gets or sets the icon of the project node.
        /// </summary>
        public Image EntryIcon { get; set; }

        /// <summary>
        /// Gets or sets the project item associated with this node.
        /// </summary>
        public ProjectItem ProjectItem { get; set; }
    }
    //// End class
}
//// End namespace
