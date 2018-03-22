namespace Squalr.Engine.Scripting
{
    using Squalr.Engine.Utils;
    using System;

    /// <summary>
    /// Class for loading scripts.
    /// </summary>
    public static class Loader
    {
        public static String CompressCompiledScript(Byte[] assembly)
        {
            return Convert.ToBase64String(Compression.Compress(assembly));
        }

        public static Byte[] DecompressCompiledScript(String script)
        {
            return Compression.Decompress(Convert.FromBase64String(script));
        }
    }
    //// End class
}
//// End namespace