namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class to contain the discovered pointers from a pointer rescan.
    /// </summary>
    public class ValidatedPointers : DiscoveredPointers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedPointers" /> class.
        /// </summary>
        public ValidatedPointers()
        {
            this.Pointers = new List<Pointer>();
        }

        /// <summary>
        /// Gets or sets the discovered pointer roots.
        /// </summary>
        public IList<Pointer> Pointers { get; set; }

        /// <summary>
        /// Gets the number of discovered pointers.
        /// </summary>
        public override UInt64 Count
        {
            get
            {
                return this.Pointers.Count.ToUInt64();
            }
        }

        /// <summary>
        /// Gets the pointers between the specified indicies.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>The pointers between the specified indicies.</returns>
        public override IEnumerable<Pointer> GetPointers(UInt64 startIndex, UInt64 endIndex)
        {
            return this.Pointers.Skip(startIndex.ToInt32()).Take((endIndex - startIndex).ToInt32());
        }

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <returns>The enumerator for the discovered pointers.</returns>
        public override IEnumerator<Pointer> GetEnumerator()
        {
            return this.Pointers.GetEnumerator();
        }
    }
    //// End class
}
//// End namespace