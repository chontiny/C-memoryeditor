namespace Squalr.Engine.Debugger.Windows.DebugEngine
{
    using Microsoft.Diagnostics.Runtime.Interop;
    using System;
    using System.Runtime.InteropServices;

    internal class EventCallBacks : IDebugEventCallbacksWide
    {
        public EventCallBacks()
        {
            this.BaseClient = null;
            this.WriteCallback = null;
            this.ReadsCallback = null;
            this.AccessesCallback = null;
        }

        public IDebugClient BaseClient { get; set; }

        private IDebugClient6 Client
        {
            get
            {
                return this.BaseClient as IDebugClient6;
            }
        }

        private IDebugControl6 Control
        {
            get
            {
                return this.BaseClient as IDebugControl6;
            }
        }

        public IDebugRegisters2 Registers
        {
            get
            {
                return this.BaseClient as IDebugRegisters2;
            }
        }

        public IDebugAdvanced3 Advanced
        {
            get
            {
                return this.BaseClient as IDebugAdvanced3;
            }
        }

        public MemoryAccessCallback WriteCallback { get; set; }

        public MemoryAccessCallback ReadsCallback { get; set; }

        public MemoryAccessCallback AccessesCallback { get; set; }

        public Int32 GetInterestMask([Out] out DEBUG_EVENT mask)
        {
            mask =// DEBUG_EVENT.BREAKPOINT
                 DEBUG_EVENT.CHANGE_DEBUGGEE_STATE
                | DEBUG_EVENT.CHANGE_ENGINE_STATE
                | DEBUG_EVENT.CHANGE_SYMBOL_STATE
                | DEBUG_EVENT.CREATE_PROCESS
                | DEBUG_EVENT.CREATE_THREAD
                | DEBUG_EVENT.EXCEPTION
                | DEBUG_EVENT.EXIT_PROCESS
                | DEBUG_EVENT.EXIT_THREAD
                | DEBUG_EVENT.LOAD_MODULE
                | DEBUG_EVENT.SESSION_STATUS
                | DEBUG_EVENT.SYSTEM_ERROR
                | DEBUG_EVENT.UNLOAD_MODULE
            ;

            return 0;
        }

        public Int32 Breakpoint([In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint2 bp)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Breakpoint Hit");
            this.Control.SetExecutionStatus(DEBUG_STATUS.GO_HANDLED);

            CodeTraceInfo codeTraceInfo = new CodeTraceInfo();

            UInt32[] registerIndicies = new UInt32[9];
            this.Registers.GetIndexByName("eax", out registerIndicies[0]);
            this.Registers.GetIndexByName("ebx", out registerIndicies[1]);
            this.Registers.GetIndexByName("ecx", out registerIndicies[2]);
            this.Registers.GetIndexByName("edx", out registerIndicies[3]);
            this.Registers.GetIndexByName("esi", out registerIndicies[4]);
            this.Registers.GetIndexByName("edi", out registerIndicies[5]);
            this.Registers.GetIndexByName("esp", out registerIndicies[6]);
            this.Registers.GetIndexByName("ebp", out registerIndicies[7]);
            this.Registers.GetIndexByName("eip", out registerIndicies[8]);

            DEBUG_VALUE[] values = new DEBUG_VALUE[9];
            this.Registers.GetValues(9, registerIndicies, 0, values);

            UInt64 offset;
            this.Registers.GetInstructionOffset(out offset);

            codeTraceInfo.Address = values[8].I64;

            return (Int32)DEBUG_STATUS.GO_NOT_HANDLED;
        }

        public Int32 Exception([In] ref EXCEPTION_RECORD64 Exception, [In] uint FirstChance)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Exception Hit");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 CreateThread([In] ulong Handle, [In] ulong DataOffset, [In] ulong StartOffset)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Thread Created");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 ExitThread([In] uint ExitCode)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Exit Thread");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 CreateProcess([In] ulong ImageFileHandle, [In] ulong Handle, [In] ulong BaseOffset, [In] uint ModuleSize, [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName, [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName, [In] uint CheckSum, [In] uint TimeDateStamp, [In] ulong InitialThreadHandle, [In] ulong ThreadDataOffset, [In] ulong StartOffset)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Debugger attached");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 ExitProcess([In] uint ExitCode)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Process exited");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 LoadModule([In] ulong ImageFileHandle, [In] ulong BaseOffset, [In] uint ModuleSize, [In, MarshalAs(UnmanagedType.LPWStr)] string ModuleName, [In, MarshalAs(UnmanagedType.LPWStr)] string ImageName, [In] uint CheckSum, [In] uint TimeDateStamp)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Module loaded");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 UnloadModule([In, MarshalAs(UnmanagedType.LPWStr)] string ImageBaseName, [In] ulong BaseOffset)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Module unloaded");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 SystemError([In] uint Error, [In] uint Level)
        {
            // Output.Output.Log(Output.LogLevel.Info, "System error");

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 SessionStatus([In] DEBUG_SESSION Status)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Session status: " + Status.ToString());

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 ChangeDebuggeeState([In] DEBUG_CDS Flags, [In] ulong Argument)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Change debuggee State: " + Flags.ToString());

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 ChangeEngineState([In] DEBUG_CES Flags, [In] ulong Argument)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Change engine State: " + Flags.ToString());

            return (Int32)DEBUG_STATUS.BREAK;
        }

        public Int32 ChangeSymbolState([In] DEBUG_CSS Flags, [In] ulong Argument)
        {
            // Output.Output.Log(Output.LogLevel.Info, "Change symbol State: " + Flags.ToString());

            return (Int32)DEBUG_STATUS.BREAK;
        }
    }
    //// End class
}
//// End namespace
