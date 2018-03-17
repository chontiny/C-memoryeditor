namespace SqualrProxy
{
    using Squalr.Assemblers.Fasm;
    using System;

    /// <summary>
    /// Proxy service to be contained by a 32 and 64 bit service, with services exposed via IPC. Useful for certain things that
    /// Squalr requires, such as:
    /// - FASM Compiler, which can only be run in 32 bit mode
    /// - Microsoft.Diagnostics.Runtime, which can only be used on processes of the same bitness
    /// </summary>
    public class ProxyAssembler : IProxyAssembler
    {
        private const Int32 AttachTimeout = 5000;

        public ProxyAssembler()
        {
        }

        public Byte[] Assemble(Boolean isProcess32Bit, String assembly, UInt64 baseAddress, out String message, out String innerMessage)
        {
            message = "Starting instruction assembly" + Environment.NewLine;
            innerMessage = String.Empty;

            if (assembly == null)
            {
                message += "No assembly code given" + Environment.NewLine;
                return null;
            }

            // Add header information about process
            if (isProcess32Bit)
            {
                assembly = String.Format("use32\n" + "org 0x{0:X8}\n", baseAddress) + assembly;
            }
            else
            {
                assembly = String.Format("use64\n" + "org 0x{0:X16}\n", baseAddress) + assembly;
            }

            message += assembly + Environment.NewLine;

            Byte[] result;
            try
            {
                // Call C++ FASM wrapper which will call the 32-bit FASM library which can assemble all x86/x64 instructions
                result = FasmNet.Assemble(assembly);

                message += "Assembled byte results:" + Environment.NewLine;

                foreach (Byte next in result)
                {
                    message += next.ToString("X") + " ";
                }

                message += Environment.NewLine;
            }
            catch (Exception ex)
            {
                innerMessage = "Error:" + ex.ToString() + Environment.NewLine;
                result = null;
            }

            return result;
        }
    }
    //// End class
}
//// End namespace