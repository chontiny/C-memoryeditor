namespace Ana.Source.Project.ProjectItems
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// A project root item which contains all project items
    /// </summary>
    [DataContract]
    internal class ProjectRoot : FolderItem
    {
        [Browsable(false)]
        private List<ProjectItem> children;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRoot" /> class
        /// </summary>
        public ProjectRoot() : base()
        {
            this.children = new List<ProjectItem>();
        }
    }
    //// End class
}
//// End namespace