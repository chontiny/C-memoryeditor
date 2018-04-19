namespace Squalr.Engine.Scanning
{
    using System;
    using System.Collections.Generic;

    public static class Scanner
    {
        /// <summary>
        /// Searches for an array of bytes in the opened process.
        /// </summary>
        /// <param name="bytes">The byte array to search for.</param>
        /// <returns>The address of the first match.</returns>
        public static IntPtr SearchAob(Byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for an array of bytes in the opened process.
        /// </summary>
        /// <param name="pattern">The string pattern to search for.</param>
        /// <returns>The address of the first match.</returns>
        public static IntPtr SearchAob(String pattern)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for an array of bytes in the opened process.
        /// </summary>
        /// <param name="pattern">The string pattern to search for.</param>
        /// <returns>The address of all matches.</returns>
        public static IEnumerable<IntPtr> SearchllAob(String pattern)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace