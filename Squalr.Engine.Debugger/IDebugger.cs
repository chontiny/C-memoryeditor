namespace Squalr.Engine.Debugger
{
    using System;

    public delegate void MemoryAccessCallback(CodeTraceInfo codeTraceInfo);

    public delegate Boolean DebugRequestCallback();

    public enum BreakpointSize
    {
        B1 = 1,
        B2 = 2,
        B4 = 4,
        B8 = 8,
    }

    public interface IDebugger
    {
        void FindWhatWrites(UInt64 address, BreakpointSize size, MemoryAccessCallback callback);

        void FindWhatReads(UInt64 address, BreakpointSize size, MemoryAccessCallback callback);

        void FindWhatAccesses(UInt64 address, BreakpointSize size, MemoryAccessCallback callback);

        Boolean IsAttached { get; }

        /// <summary>
        /// Gets or sets the debug request callback. If this is set, this callback will be called before the debugger is attached.
        /// The debugger will only perform the attach if the result of the callback is true.
        /// </summary>
        DebugRequestCallback DebugRequestCallback { get; set; }
    }
    //// End interface
}
//// End namespace
