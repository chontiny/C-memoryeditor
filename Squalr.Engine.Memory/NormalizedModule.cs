namespace Squalr.Engine.Memory
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Defines an OS independent module region and attributes.
    /// </summary>
    public class NormalizedModule : NormalizedRegion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedModule" /> class.
        /// </summary>
        /// <param name="fullPath">The path of the module.</param>
        /// <param name="baseAddress">The base address of the module.</param>
        /// <param name="size">The total size of the module.</param>
        public NormalizedModule(String fullPath, UInt64 baseAddress, Int32 size) : base(baseAddress, size)
        {
            this.Name = Path.GetFileName(fullPath);
            this.FullPath = fullPath;

            DirectoryInfo modulePathInfo = new DirectoryInfo(fullPath);
            DirectoryInfo systemPathInfo = new DirectoryInfo(Environment.SystemDirectory);

            // Ignore system paths
            if (modulePathInfo.Parent == systemPathInfo)
            {
                this.DataSegments = MetaData.Default.GetDataSegments(baseAddress, fullPath);
            }
            else
            {
                this.DataSegments = new List<NormalizedRegion>();
            }
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Gets the full path of the module.
        /// </summary>
        public String FullPath { get; private set; }

        /// <summary>
        /// Gets the list of data segments in the module in the scope of the current virtual memory layout.
        /// </summary>
        public IList<NormalizedRegion> DataSegments { get; private set; }
    }
    //// End interface
}
//// End namespace