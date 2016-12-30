namespace Ana.Source.Utils
{
    using System;
    using System.IO;
    using System.IO.Compression;

    internal class Compression
    {
        public static Byte[] Compress(Byte[] bytes)
        {
            using (MemoryStream memoryStreamInput = new MemoryStream(bytes))
            {
                using (MemoryStream memoryStreamOutput = new MemoryStream())
                {
                    using (GZipStream gZipStream = new GZipStream(memoryStreamOutput, CompressionMode.Compress))
                    {
                        memoryStreamInput.CopyTo(gZipStream);
                    }

                    return memoryStreamOutput.ToArray();
                }
            }
        }

        public static Byte[] Decompress(Byte[] bytes)
        {
            using (MemoryStream memoryStreamInput = new MemoryStream(bytes))
            {
                using (MemoryStream memoryStreamOutput = new MemoryStream())
                {
                    using (GZipStream gZipStream = new GZipStream(memoryStreamInput, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(memoryStreamOutput);
                    }

                    return memoryStreamOutput.ToArray();
                }
            }
        }
    }
    //// End class
}
//// End namespace