namespace SqualrProxy
{
    using System;
    using System.ServiceModel;

    [ServiceContract()]
    public interface IProxyAssembler
    {
        [OperationContract(IsOneWay = true)]
        Byte[] Assemble(Boolean isProcess32Bit, String assembly, UInt64 baseAddress, out String message, out String innerMessage);
    }
    //// End class
}
//// End namespace