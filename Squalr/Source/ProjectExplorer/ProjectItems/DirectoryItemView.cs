namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class DirectoryItemView : ProjectItemView
    {
        public DirectoryItemView(DirectoryItem directoryItem)
        {
            this.DirectoryItem = directoryItem;
        }

        public String Path
        {
            get
            {
                return this.DirectoryItem.Path;
            }

            set
            {
                this.DirectoryItem.Path = value;
            }
        }

        [Browsable(false)]
        private DirectoryItem DirectoryItem { get; set; }
    }
    //// End class
}
//// End namespace