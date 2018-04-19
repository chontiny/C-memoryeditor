namespace Squalr.Engine.Memory
{
    using Processes;
    using Squalr.Engine.DataTypes;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for reading virtual memory.
    /// </summary>
    public interface IMemoryReader : IProcessObserver
    {
        /// <summary>
        /// Reads a value from the opened processes memory.
        /// </summary>
        /// <param name="elementType">The data type to read.</param>
        /// <param name="address">The address to read from.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>The value read from memory.</returns>
        Object Read(DataType elementType, IntPtr address, out Boolean success);

        /// <summary>
        /// Reads a value from the opened processes memory.
        /// </summary>
        /// <typeparam name="T">The data type to read.</typeparam>
        /// <param name="address">The address to read from.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>The value read from memory.</returns>
        T Read<T>(IntPtr address, out Boolean success);

        /// <summary>
        /// Reads an array of bytes from the opened processes memory.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>The array of bytes read from memory, if the read succeeded.</returns>
        Byte[] ReadBytes(IntPtr address, Int32 count, out Boolean success);

        IntPtr EvaluatePointer(IntPtr address, IEnumerable<Int32> offsets);
    }
    //// End interface
}
//// End namespace