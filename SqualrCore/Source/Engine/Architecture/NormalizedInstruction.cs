namespace SqualrCore.Source.Engine.Architecture
{
    using System;

    /// <summary>
    /// Object that represents a platform agnostic instruction.
    /// </summary>
    public class NormalizedInstruction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedInstruction" /> class.
        /// </summary>
        /// <param name="instruction">The instruction string.</param>
        /// <param name="size">The instruction size.</param>
        public NormalizedInstruction(String instruction, Byte[] bytes, Int32 size)
        {
            this.Instruction = instruction;
            this.Bytes = bytes;
            this.Size = size;
        }

        /// <summary>
        /// Gets the instruction string.
        /// </summary>
        public String Instruction { get; private set; }

        /// <summary>
        /// Gets the instruction bytes.
        /// </summary>
        public Byte[] Bytes { get; private set; }

        /// <summary>
        /// Gets the size of this instruction.
        /// </summary>
        public Int32 Size { get; private set; }
    }
    /// End class
}
/// End namespace