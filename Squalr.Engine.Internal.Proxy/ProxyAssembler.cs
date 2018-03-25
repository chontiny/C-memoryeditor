namespace SqualrProxy
{
    using Squalr.Engine.Internal.Fasm;
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

        public AssemblerResult Assemble(Boolean isProcess32Bit, String assembly, UInt64 baseAddress)
        {
            AssemblerResult result = new AssemblerResult(null, "", "");

            result.Message = "Starting instruction assembly" + Environment.NewLine;
            result.InnerMessage = String.Empty;

            if (assembly == null)
            {
                result.Message += "No assembly code given" + Environment.NewLine;
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

            result.Message += assembly + Environment.NewLine;

            try
            {
                // Call C++ FASM wrapper which will call the 32-bit FASM library which can assemble all x86/x64 instructions
                result.Data = FasmNet.Assemble(assembly);

                result.Message += "Assembled byte results:" + Environment.NewLine;

                foreach (Byte next in result.Data)
                {
                    result.Message += next.ToString("X") + " ";
                }

                result.Message += Environment.NewLine;
            }
            catch (Exception ex)
            {
                result.InnerMessage = "Error:" + ex.ToString() + Environment.NewLine;
                result = null;
            }

            return result;
        }
    }
    //// End class
}
//// End namespace