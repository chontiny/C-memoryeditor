namespace Ana.View.Controls.TreeView
{
    using Aga.Controls.Tree;
    using Source.Project.ProjectItems;
    using System;
    using System.Drawing;

    /// <summary>
    /// Defines a node in the project explorer
    /// </summary>
    internal class ProjectNode : Node
    {
        public String EntryDescription { get; set; }
        public String EntryValuePreview { get; set; }
        public Image EntryIcon { get; set; }
        public ProjectItem ProjectItem { get; set; }

        /// <summary>
        /// Initializes a new MyNode class with a given Text property.
        /// </summary>
        /// <param name="text">String to set the text property with.</param>
        public ProjectNode(String entryDescription) : base(String.Empty)
        {
            this.EntryDescription = entryDescription;
            this.EntryValuePreview = String.Empty;

            EntryIcon = null;
        }

    }
    //// End class
}
//// End namespace
