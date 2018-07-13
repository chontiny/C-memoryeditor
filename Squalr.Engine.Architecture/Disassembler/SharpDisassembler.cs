namespace Squalr.Engine.Architecture.Disassemblers
{
    using SharpDisasm;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public Architecture.Instruction[] Disassemble(Byte[] bytes, Boolean isProcess32Bit, UInt64 baseAddress)
        {
            this.disassembler = new Disassembler(
                code: bytes,
                architecture: isProcess32Bit ? ArchitectureMode.x86_32 : ArchitectureMode.x86_64,
                address: baseAddress,
                copyBinaryToInstruction: true);

            IEnumerable<Instruction> instructions = this.disassembler.Disassemble();

            return instructions.Select(instruction =>
                new Architecture.Instruction(
                    instruction.Offset,
                    instruction.ToString(),
                    instruction.Bytes,
                    instruction.Bytes.Length)).ToArray();
        }
    }
    //// End class
}
//// End namespace