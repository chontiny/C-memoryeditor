namespace Squalr.Engine.Debugger.Windows.DebugEngine
{
    using Microsoft.Diagnostics.Runtime.Interop;
    using System;
    using System.Runtime.InteropServices;

    internal class EventCallBacks : IDebugEventCallbacksWide
    {
        public EventCallBacks()
        {
            this.Control = null;
            this.WriteCallback = null;
            this.ReadsCallback = null;
            this.AccessesCallback = null;
        }

        public IDebugControl6 Control { get; set; }

        public MemoryAccessCallback WriteCallback { get; set; }

        public MemoryAccessCallback ReadsCallback { get; set; }

        public MemoryAccessCallback AccessesCallback { get; set; }

        public int GetInterestMask([Out] out DEBUG_EVENT mask)
        {
            mask = DEBUG_EVENT.BREAKPOINT
              // | Enums.DebugEvent.ChangeDebuggeeState
              // | Enums.DebugEvent.ChangeEngineState
              // | Enums.DebugEvent.ChangeSymbolState
              | DEBUG_EVENT.CREATE_PROCESS
              | DEBUG_EVENT.CREATE_THREAD
            // | Enums.DebugEvent.Exception
            // | Enums.DebugEvent.ExitProcess
            // | Enums.DebugEvent.ExitThread
            // | Enums.DebugEvent.LoadModule
            // | Enums.DebugEvent.SessionStatus
            // | Enums.DebugEvent.SystemError
            // | Enums.DebugEvent.UnloadModule
            ;

            return 0;
        }

        public int Breakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 bp)
        {
            Output.Output.Log(Output.LogLevel.Info, "Breakpoint Hit");
            this.Control.SetExecutionStatus(DEBUG_STATUS.GO_HANDLED);

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public int Exception([In] ref EXCEPTION_RECORD64 Exception, [In] uint FirstChance)
        {
            Output.Output.Log(Output.LogLevel.Info, "Exception Hit");

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int CreateThread([In] ulong Handle, [In] ulong DataOffset, [In] ulong StartOffset)
        {
            Output.Output.Log(Output.LogLevel.Info, "Thread Created");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public int ExitThread([In] uint ExitCode)
        {
            Output.Output.Log(Output.LogLevel.Info, "Exit Thread");

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int CreateProcess([In] ulong ImageFileHandle, [In] ulong Handle, [In] ulong BaseOffset, [In] uint ModuleSize, [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName, [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName, [In] uint CheckSum, [In] uint TimeDateStamp, [In] ulong InitialThreadHandle, [In] ulong ThreadDataOffset, [In] ulong StartOffset)
        {
            Output.Output.Log(Output.LogLevel.Info, "Debugger attached");


            return (Int32)DEBUG_STATUS.BREAK;
        }

        public int ExitProcess([In] uint ExitCode)
        {
            Output.Output.Log(Output.LogLevel.Info, "Process exited");

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int LoadModule([In] ulong ImageFileHandle, [In] ulong BaseOffset, [In] uint ModuleSize, [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName, [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName, [In] uint CheckSum, [In] uint TimeDateStamp)
        {
            Output.Output.Log(Output.LogLevel.Info, "Module loaded");

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int UnloadModule([In, MarshalAs(UnmanagedType.LPWStr)] string ImageBaseName, [In] ulong BaseOffset)
        {
            Output.Output.Log(Output.LogLevel.Info, "Module unloaded");

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int SystemError([In] uint Error, [In] uint Level)
        {
            Output.Output.Log(Output.LogLevel.Info, "System error");

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int SessionStatus([In] DEBUG_SESSION Status)
        {
            Output.Output.Log(Output.LogLevel.Info, "Session status: " + Status.ToString());

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int ChangeDebuggeeState([In] DEBUG_CDS Flags, [In] ulong Argument)
        {
            Output.Output.Log(Output.LogLevel.Info, "Change debuggee State: " + Flags.ToString());

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int ChangeEngineState([In] DEBUG_CES Flags, [In] ulong Argument)
        {
            Output.Output.Log(Output.LogLevel.Info, "Change engine State: " + Flags.ToString());

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }

        public int ChangeSymbolState([In] DEBUG_CSS Flags, [In] ulong Argument)
        {
            Output.Output.Log(Output.LogLevel.Info, "Change symbol State: " + Flags.ToString());

            return (Int32)DEBUG_STATUS.NO_CHANGE;
        }
    }
    //// End class
}
//// End namespace
