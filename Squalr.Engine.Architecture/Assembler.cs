namespace Squalr.Engine.Architecture
{
    using Squalr.Engine.Architecture.Assemblers;

    public static class Assembler
    {
        /// <summary>
        /// Gets the default assembler, based on the underlying system.
        /// </summary>
        public static IAssembler Default
        {
            get
            {
                return AssemblerFactory.GetAssembler(ArchitectureType.x86_64);
            }
        }

        /// <summary>
        /// Gets an x86/x64 assembler.
        /// </summary>
        public static IAssembler X86_64
        {
            get
            {
                return AssemblerFactory.GetAssembler(ArchitectureType.x86_64);
            }
        }
    }
    //// End class
}
//// End namespace