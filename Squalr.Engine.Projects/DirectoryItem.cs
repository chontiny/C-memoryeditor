namespace Squalr.Engine.Projects
{
    using System;

    /// <summary>
    /// Defines a directory in the project.
    /// </summary>
    public class DirectoryItem : ProjectItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        public DirectoryItem(String path) : base(path)
        {
            this.Path = path;
        }

        public override void Update()
        {
        }
    }
    //// End class
}
//// End namespace