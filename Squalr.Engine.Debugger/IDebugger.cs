namespace Squalr.Engine.Debugger
{
    using System;

    public delegate void MemoryAccessCallback();

    public interface IDebugger
    {
        void FindWhatWrites(UInt64 address, MemoryAccessCallback callback);

        void FindWhatReads(UInt64 address, MemoryAccessCallback callback);

        void FindWhatAccesses(UInt64 address, MemoryAccessCallback callback);
    }
    //// End interface
}
//// End namespace
