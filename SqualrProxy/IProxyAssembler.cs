namespace SqualrProxy
{
    using System;
    using System.ServiceModel;

    [ServiceContract()]
    public interface IProxyAssembler
    {
        [OperationContract]
        Byte[] Assemble(Boolean isProcess32Bit, String assembly, UInt64 baseAddress, out String logs);
    }
    //// End class
}
//// End namespace