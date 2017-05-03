namespace Squalr.Source.Editors.StreamIconEditor
{
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using System;
    using System.Windows.Media.Imaging;

    internal class StreamTableItem
    {
        public StreamTableItem(ProjectItem projectItem, StreamIcon streamIcon)
        {
            this.StreamCommand = "!" + projectItem.StreamCommand;
            this.Description = projectItem.Description;
            this.ExtendedDescription = projectItem.ExtendedDescription;
            this.Category = projectItem.Category;
            this.Icon = streamIcon.Icon;
        }

        /// <summary>
        /// 
        /// </summary>
        public String StreamCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public String Description { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public String ExtendedDescription { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ProjectItem.ProjectItemCategory Category { get; private set; }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        public BitmapImage Icon { get; private set; }
    }
    //// End class
}
//// End namespace