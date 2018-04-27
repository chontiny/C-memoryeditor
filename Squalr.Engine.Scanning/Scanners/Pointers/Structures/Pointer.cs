using System;
using System.Collections.Generic;

namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    public class Pointer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address. This will be added as an offset from the resolved base identifier.</param>
        /// <param name="dataType">The data type of the value at this address.</param>
        /// <param name="description">The description of this address.</param>
        /// <param name="resolveType">The identifier type for this address item.</param>
        /// <param name="moduleName">The identifier for the base address of this object.</param>
        /// <param name="pointerOffsets">The pointer offsets of this address item.</param>
        public Pointer(
            IntPtr baseAddress,
            Type dataType,
            String description = "New Address",
            String moduleName = null,
            IEnumerable<Int32> pointerOffsets = null)
        {
        }

        /// <summary>
        /// Gets the effective address after tracing all pointer offsets.
        /// </summary>
        public IntPtr CalculatedAddress { get; set; }

        public Object AddressValue { get; set; }

        public void Update()
        {

        }
    }
    //// End class
}
//// End namespace