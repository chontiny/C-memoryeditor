namespace SqualrProxy
{
    using System;

    public interface IProxyAssembler
    {
        AssemblerResult Assemble(Boolean isProcess32Bit, String assembly, UInt64 baseAddress);
    }
    //// End class
}
//// End namespace