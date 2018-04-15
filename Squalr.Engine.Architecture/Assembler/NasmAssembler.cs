namespace Squalr.Engine.Architecture.Assembler
{
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// The Nasm assembler for x86/64.
    /// </summary>
    internal class NasmAssembler : IAssembler
    {
        /// <summary>
        /// The 32 bit proxy service executable
        /// </summary>
        private const String ExecutablePath = "Library/nasm.exe";

        /// <summary>
        /// Assemble the specified assembly code.
        /// </summary>
        /// <param name="isProcess32Bit">Whether or not the assembly is in the context of a 32 bit program.</param>
        /// <param name="assembly">The assembly code.</param>
        /// <returns>An array of bytes containing the assembly code.</returns>
        public AssemblerResult Assemble(Boolean isProcess32Bit, String assembly)
        {
            // Assemble and return the code
            return this.Assemble(isProcess32Bit, assembly, IntPtr.Zero);
        }

        /// <summary>
        /// Assemble the specified assembly code at a base address.
        /// </summary>
        /// <param name="isProcess32Bit">Whether or not the assembly is in the context of a 32 bit program.</param>
        /// <param name="assembly">The assembly code.</param>
        /// <param name="baseAddress">The address where the code is rebased.</param>
        /// <returns>An array of bytes containing the assembly code.</returns>
        public AssemblerResult Assemble(Boolean isProcess32Bit, String assembly, IntPtr baseAddress)
        {
            AssemblerResult result = new AssemblerResult();
            String preamble = "org 0x" + baseAddress.ToString("X") + Environment.NewLine;

            if (isProcess32Bit)
            {
                preamble += "[BITS 32]" + Environment.NewLine;
            }
            else
            {
                preamble += "[BITS 64]" + Environment.NewLine;
            }

            assembly = preamble + assembly;

            try
            {
                String assemblyFilePath = Path.Combine(Path.GetTempPath(), "SqualrAssembly" + Guid.NewGuid() + ".asm");
                String outputFilePath = Path.Combine(Path.GetTempPath(), "SqualrAssembly" + Guid.NewGuid() + ".bin");

                File.WriteAllText(assemblyFilePath, assembly);
                String exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), NasmAssembler.ExecutablePath);
                StringBuilder buildOutput = new StringBuilder();
                ProcessStartInfo startInfo = new ProcessStartInfo(exePath);
                startInfo.Arguments = "-f bin -o " + NasmAssembler.Escape(outputFilePath) + " " + NasmAssembler.Escape(assemblyFilePath);
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                Process process = Process.Start(startInfo);
                result.Message = process.StandardOutput.ReadToEnd();
                result.InnerMessage = process.StandardError.ReadToEnd();

                if (result.Message.IsNullOrEmpty() && !result.InnerMessage.IsNullOrEmpty())
                {
                    result.Message = "NASM Compile error";
                }

                process.WaitForExit();

                if (File.Exists(outputFilePath))
                {
                    result.Data = File.ReadAllBytes(outputFilePath);
                }
            }
            catch (Exception ex)
            {
                result.Message = "Error compiling with NASM";
                result.InnerMessage = ex.ToString();
            }

            return result;
        }

        private static String Escape(String str)
        {
            return String.Format("\"{0}\"", str);
        }
    }
    //// End class
}
//// End namespace