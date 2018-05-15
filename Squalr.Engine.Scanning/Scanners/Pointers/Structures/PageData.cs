namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A class to contain the discovered pointers from a pointer scan.
    /// </summary>
    public class PageData : IEnumerable<Pointer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageData" /> class.
        /// </summary>
        public PageData()
        {
        }

        public IEnumerator<Pointer> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace