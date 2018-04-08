namespace Squalr.Engine.Debugger.Windows.DebugEngine
{
    using Microsoft.Diagnostics.Runtime.Interop;
    using Squalr.Engine.Processes;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DebugEngine : IDebugger, IProcessObserver
    {
        public DebugEngine()
        {
            this.DebugRequestCallback = null;
            this.EventCallBacks = new EventCallBacks();
            this.OutputCallBacks = new OutputCallBacks();
            this.Scheduler = new ConcurrentExclusiveSchedulerPair();
            this.Inturrupt = false;

            ProcessAdapterFactory.GetProcessAdapter().Subscribe(this);
        }

        /// <summary>
        /// Gets or sets the debug request callback. This callback will be called before the debugger is attached,
        /// and will only be attached if the result of the callback is true.
        /// </summary>
        public DebugRequestCallback DebugRequestCallback { get; set; }

        public Boolean IsAttached { get; set; }

        private IDebugClient6 Client { get; set; }

        private IDebugControl6 Control { get; set; }

        private Process SystemProcess { get; set; }

        private EventCallBacks EventCallBacks { get; set; }

        private OutputCallBacks OutputCallBacks { get; set; }

        private ConcurrentExclusiveSchedulerPair Scheduler { get; set; }

        private Boolean Inturrupt { get; set; }

        public void Update(NormalizedProcess process)
        {
            if (process == null)
            {
                return;
            }

            this.SystemProcess = Process.GetProcessById(process.ProcessId);
        }

        public void FindWhatWrites(UInt64 address, BreakpointSize size, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = callback;
            this.EventCallBacks.ReadsCallback = null;
            this.EventCallBacks.AccessesCallback = null;

            this.SetHardwareBreakpoint(address, DEBUG_BREAKPOINT_ACCESS_TYPE.WRITE, size.ToUInt32());
        }

        public void FindWhatReads(UInt64 address, BreakpointSize size, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = null;
            this.EventCallBacks.ReadsCallback = callback;
            this.EventCallBacks.AccessesCallback = null;

            this.SetHardwareBreakpoint(address, DEBUG_BREAKPOINT_ACCESS_TYPE.READ, size.ToUInt32());
        }

        public void FindWhatAccesses(UInt64 address, BreakpointSize size, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = null;
            this.EventCallBacks.ReadsCallback = null;
            this.EventCallBacks.AccessesCallback = callback;

            this.SetHardwareBreakpoint(address, DEBUG_BREAKPOINT_ACCESS_TYPE.READ | DEBUG_BREAKPOINT_ACCESS_TYPE.WRITE, size.ToUInt32());
        }

        public void Attach()
        {
            // Exit if already attached, or debug request fails
            if (this.IsAttached || (this.DebugRequestCallback != null && this.DebugRequestCallback()))
            {
                return;
            }

            // Perform the attach
            Task.Factory.StartNew(() =>
            {
                try
                {
                    IDebugClient baseClient = DebugEngine.CreateIDebugClient();

                    this.Client = baseClient as IDebugClient6;
                    this.Control = baseClient as IDebugControl6;

                    OutputCallBacks outputCallBacks = new OutputCallBacks();
                    EventCallBacks eventCallBacks = new EventCallBacks();

                    eventCallBacks.Control = this.Control;

                    this.Client.SetOutputCallbacksWide(outputCallBacks);
                    this.Client.SetEventCallbacksWide(eventCallBacks);

                    this.Client.AttachProcess(0, unchecked((UInt32)this.SystemProcess.Id), DEBUG_ATTACH.DEFAULT);
                    this.Control.WaitForEvent(0, 0);

                    this.IsAttached = true;
                }
                catch (Exception ex)
                {
                    Output.Output.Log(Output.LogLevel.Error, "Error attaching debugger", ex);
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, Scheduler.ExclusiveScheduler).Wait();

            // Listen for events such as breakpoint hits
            this.ListenForEvents();
        }

        private void ListenForEvents()
        {
            Task.Run(() =>
            {
                while (this.IsAttached)
                {
                    if (!this.Inturrupt)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                DEBUG_STATUS status;

                                while (true)
                                {

                                    this.Control.GetExecutionStatus(out status);

                                    if (status == DEBUG_STATUS.NO_DEBUGGEE)
                                    {
                                        break;
                                    }

                                    if (status == DEBUG_STATUS.GO || status == DEBUG_STATUS.BREAK || status == DEBUG_STATUS.STEP_BRANCH || status == DEBUG_STATUS.STEP_INTO || status == DEBUG_STATUS.STEP_OVER)
                                    {
                                        Int32 hr = this.Control.WaitForEvent(DEBUG_WAIT.DEFAULT, UInt32.MaxValue);
                                        continue;
                                    }
                                }
                                // this.Control.WaitForEvent(0, UInt32.MaxValue);
                            }
                            catch (Exception ex)
                            {
                                Output.Output.Log(Output.LogLevel.Error, "Error listening for debugger events", ex);
                            }
                        }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, Scheduler.ExclusiveScheduler);
                    }
                }
            });
        }

        private IDebugBreakpoint2 SetSoftwareBreakpoint(UInt64 address, DEBUG_BREAKPOINT_ACCESS_TYPE access, UInt32 size)
        {
            return this.SetBreakpoint(address, DEBUG_BREAKPOINT_TYPE.CODE, access, size);
        }

        private IDebugBreakpoint2 SetHardwareBreakpoint(UInt64 address, DEBUG_BREAKPOINT_ACCESS_TYPE access, UInt32 size)
        {
            return this.SetBreakpoint(address, DEBUG_BREAKPOINT_TYPE.DATA, access, size);
        }

        private IDebugBreakpoint2 SetBreakpoint(UInt64 address, DEBUG_BREAKPOINT_TYPE breakpointType, DEBUG_BREAKPOINT_ACCESS_TYPE access, UInt32 size)
        {
            const UInt32 AnyId = UInt32.MaxValue;

            IDebugBreakpoint2 breakpoint = null;

            this.BeginInturrupt();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    int hResult = this.Control.AddBreakpoint2(breakpointType, AnyId, out breakpoint);

                    if (hResult < 0)
                    {
                        throw new Exception("Invalid HRESULT: " + hResult.ToString());
                    }

                    breakpoint.SetOffset(address);
                    breakpoint.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
                    breakpoint.SetDataParameters(size, access);
                }
                catch (Exception ex)
                {
                    Output.Output.Log(Output.LogLevel.Error, "Error setting breakpoint", ex);
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, Scheduler.ExclusiveScheduler);

            this.EndInturrupt();

            return breakpoint;
        }

        private void BeginInturrupt()
        {
            this.Inturrupt = true;
            this.Control.SetInterrupt(DEBUG_INTERRUPT.EXIT);
        }

        private void EndInturrupt()
        {
            this.Inturrupt = false;
        }

        private static IDebugClient CreateIDebugClient()
        {
            Guid guid = typeof(IDebugClient).GUID;
            DebugEngine.DebugCreate(ref guid, out Object obj);

            IDebugClient client = (IDebugClient)obj;
            return client;
        }

        /// <summary>
        /// The DebugCreate function creates a new client object and returns an interface pointer to it.
        /// </summary>
        /// <param name="InterfaceId">Specifies the interface identifier (IID) of the desired debugger engine client interface. This is the type of the interface that will be returned in Interface.</param>
        /// <param name="Interface">Receives an interface pointer for the new client. The type of this interface is specified by InterfaceId.</param>
        [DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
        [DllImport("dbgeng.dll")]
        internal static extern UInt32 DebugCreate(ref Guid InterfaceId, [MarshalAs(UnmanagedType.IUnknown)] out Object Interface);
    }
    //// End class
}
//// End namespace