namespace Squalr.Engine.Debugger
{
    using System;

    public delegate void MemoryAccessCallback(CodeTraceInfo codeTraceInfo);
    public delegate Boolean DebugRequestCallback();

    public interface IDebugger
    {
        void FindWhatWrites(UInt64 address, MemoryAccessCallback callback);

        void FindWhatReads(UInt64 address, MemoryAccessCallback callback);

        void FindWhatAccesses(UInt64 address, MemoryAccessCallback callback);

        Boolean IsAttached { get; set; }

        /// <summary>
        /// Gets or sets the debug request callback. This callback will be called before the debugger is attached,
        /// and will only be attached if the result of the callback is true.
        /// </summary>
        DebugRequestCallback DebugRequestCallback { get; set; }
    }
    //// End interface
}
//// End namespace
