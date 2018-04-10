namespace Squalr.Engine.Debugger
{
    using System;
    using System.Threading;

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
        CancellationTokenSource FindWhatReads(UInt64 address, BreakpointSize size, MemoryAccessCallback callback);

        CancellationTokenSource FindWhatWrites(UInt64 address, BreakpointSize size, MemoryAccessCallback callback);

        CancellationTokenSource FindWhatAccesses(UInt64 address, BreakpointSize size, MemoryAccessCallback callback);

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
