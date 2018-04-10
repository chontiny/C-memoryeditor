namespace Squalr.Engine.Debugger.Windows.DebugEngine
{
    using Microsoft.Diagnostics.Runtime.Interop;
    using Squalr.Engine.Output;
    using System;
    using System.Runtime.InteropServices;

    internal class OutputCallBacks : IDebugOutputCallbacksWide
    {
        public Int32 Output([In] DEBUG_OUTPUT Mask, [In, MarshalAs(UnmanagedType.LPWStr)] String text)
        {
            Engine.Output.Output.Log(LogLevel.Debug, text?.Trim());

            return 0;
        }
    }
    //// End class
}
//// End namespace
