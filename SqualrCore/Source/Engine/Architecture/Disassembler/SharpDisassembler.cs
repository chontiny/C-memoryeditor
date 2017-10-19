namespace SqualrCore.Source.Engine.Architecture.Disassembler
{
    using SharpDisasm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utils.Extensions;

    /// <summary>
    /// An implementation of the udis disassembler.
    /// </summary>
    internal class SharpDisassembler : IDisassembler
    {
        /// <summary>
        /// An instruction disassembler.
        /// </summary>
        private Disassembler disassembler;

        /// <summary>
        /// Disassemble the specified bytes.
        /// </summary>
        /// <param name="bytes">The bytes to be disassembled.</param>
        /// <param name="isProcess32Bit">Whether or not the assembly is in the context of a 32 bit program.</param>
        /// <param name="baseAddress">The address where the code is rebased.</param>
        /// <returns>An array of bytes containing the assembly code.</returns>
        public IEnumerable<NormalizedInstruction> Disassemble(Byte[] bytes, Boolean isProcess32Bit, IntPtr baseAddress)
        {
            this.disassembler = new Disassembler(bytes, isProcess32Bit ? ArchitectureMode.x86_32 : ArchitectureMode.x86_64, baseAddress.ToUInt64());
            IEnumerable<Instruction> instructions = this.disassembler.Disassemble();

            return instructions.Select(
                instruction => new NormalizedInstruction(instruction.ToString(), instruction.Bytes, instruction.Bytes.Length)
            );
        }
    }
    //// End class
}
//// End namespace