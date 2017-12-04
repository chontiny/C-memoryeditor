namespace SqualrCore.Source.Engine.Debugger.Windows.DebugEngine
{
    using DbgEng;
    using SqualrCore.Source.Engine.Processes;
    using SqualrCore.Source.Output;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal class DebugEngine : IDebugger, IProcessObserver
    {
        public DebugEngine()
        {
            Task.Run(() => EngineCore.GetInstance().Processes.Subscribe(this));
        }

        private IDebugClient Client { get; set; }

        private Process SystemProcess { get; set; }

        public void Update(NormalizedProcess process)
        {
            if (process == null)
            {
                return;
            }

            this.SystemProcess = Process.GetProcessById(process.ProcessId);
        }

        public void Attach()
        {
            Task.Run(() =>
            {
                this.Client = DebugClient.DebugCreate();

                IDebugClient7 client = this.Client as IDebugClient7;
                IDebugControl6 control = this.Client as IDebugControl6;

                client.SetOutputCallbacksWide(new OutputCallBacks());
                client.SetEventCallbacksWide(new EventCallBacks(control));


                this.Client.AttachProcess(0, unchecked((UInt32)this.SystemProcess.Id), DebugAttach.InvasiveNoInitialBreak);

                while (true)
                {
                    control.WaitForEvent(0, UInt32.MaxValue);
                }
            });
        }
    }

    internal class EventCallBacks : IDebugEventCallbacksWide
    {
        public EventCallBacks(IDebugControl6 control)
        {
            this.Control = control;
        }


        private enum DEBUG_BREAKPOINT_TYPE : uint
        {
            CODE = 0,
            DATA = 1,
            TIME = 2,
        }

        [Flags]
        private enum DEBUG_BREAKPOINT_FLAG : uint
        {
            GO_ONLY = 1,
            DEFERRED = 2,
            ENABLED = 4,
            ADDER_ONLY = 8,
            ONE_SHOT = 0x10,
        }

        private enum DEBUG_STATUS : uint
        {
            NO_CHANGE = 0,
            GO = 1,
            GO_HANDLED = 2,
            GO_NOT_HANDLED = 3,
            STEP_OVER = 4,
            STEP_INTO = 5,
            BREAK = 6,
            NO_DEBUGGEE = 7,
            STEP_BRANCH = 8,
            IGNORE_EVENT = 9,
            RESTART_REQUESTED = 10,
            REVERSE_GO = 11,
            REVERSE_STEP_BRANCH = 12,
            REVERSE_STEP_OVER = 13,
            REVERSE_STEP_INTO = 14,
            OUT_OF_SYNC = 15,
            WAIT_INPUT = 16,
            TIMEOUT = 17,
            MASK = 0x1f,
        }

        private IDebugControl6 Control { get; set; }

        public uint GetInterestMask()
        {
            return (UInt32)(DebugEvent.Breakpoint
                | DebugEvent.ChangeDebuggeeState
                | DebugEvent.ChangeEngineState
                | DebugEvent.ChangeSymbolState
                | DebugEvent.CreateProcess
                | DebugEvent.CreateThread
                | DebugEvent.Exception
                | DebugEvent.ExitProcess
                | DebugEvent.ExitThread
                | DebugEvent.LoadModule
                | DebugEvent.SessionStatus
                | DebugEvent.SystemError
                | DebugEvent.UnloadModule);
        }

        public void Breakpoint(IDebugBreakpoint2 Bp)
        {
            Control.SetExecutionStatus((UInt32)DEBUG_STATUS.GO_HANDLED);
        }

        public void Exception(ref _EXCEPTION_RECORD64 Exception, uint FirstChance)
        {

        }

        public void CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset)
        {
        }

        public void ExitThread(uint ExitCode)
        {
        }

        public void CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName = null, string ImageName = null, uint CheckSum = 0, uint TimeDateStamp = 0, ulong InitialThreadHandle = 0, ulong ThreadDataOffset = 0, ulong StartOffset = 0)
        {
            const Int32 Software = 0;
            const Int32 Hardware = 1;
            const UInt32 AnyId = UInt32.MaxValue;

            IDebugBreakpoint bp = this.Control.AddBreakpoint2(Software, AnyId);

            bp.SetOffset(0x1003830);
            bp.SetFlags((UInt32)DEBUG_BREAKPOINT_FLAG.ENABLED);
        }

        public void ExitProcess(uint ExitCode)
        {
        }

        public void LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName = null, string ImageName = null, uint CheckSum = 0, uint TimeDateStamp = 0)
        {
        }

        public void UnloadModule(string ImageBaseName = null, ulong BaseOffset = 0)
        {
        }

        public void SystemError(uint Error, uint Level)
        {
        }

        public void SessionStatus(uint Status)
        {
        }

        public void ChangeDebuggeeState(uint Flags, ulong Argument)
        {
        }

        public void ChangeEngineState(uint Flags, ulong Argument)
        {
        }

        public void ChangeSymbolState(uint Flags, ulong Argument)
        {
        }
    }

    internal class OutputCallBacks : IDebugOutputCallbacksWide
    {
        public void Output(UInt32 mask, String text)
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, text?.Trim());
        }
    }
    //// End class
}
//// End namespace