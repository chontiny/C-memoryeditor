namespace Ana.Source.Project.ProjectItems
{
    using System.Runtime.Serialization;

    /// <summary>
    /// A project root item which contains all project items.
    /// </summary>
    [DataContract]
    internal class ProjectRoot : FolderItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRoot" /> class.
        /// </summary>
        public ProjectRoot() : base("Project Root")
        {
        }
    }
    //// End class
}
//// End namespace