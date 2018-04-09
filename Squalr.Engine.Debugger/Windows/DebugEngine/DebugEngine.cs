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

            ProcessAdapterFactory.GetProcessAdapter().Subscribe(this);
        }

        /// <summary>
        /// Gets or sets the debug request callback. This callback will be called before the debugger is attached,
        /// and will only be attached if the result of the callback is true.
        /// </summary>
        public DebugRequestCallback DebugRequestCallback { get; set; }

        public Boolean IsAttached { get; set; }

        private IDebugClient BaseClient { get; set; }

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

        private Process SystemProcess { get; set; }

        private EventCallBacks EventCallBacks { get; set; }

        private OutputCallBacks OutputCallBacks { get; set; }

        private ConcurrentExclusiveSchedulerPair Scheduler { get; set; }

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
            this.EventCallBacks.ReadCallback = null;
            this.EventCallBacks.AccessCallback = null;

            this.SetHardwareBreakpoint(address, DEBUG_BREAKPOINT_ACCESS_TYPE.WRITE, size.ToUInt32());
        }

        public void FindWhatReads(UInt64 address, BreakpointSize size, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = null;
            this.EventCallBacks.ReadCallback = callback;
            this.EventCallBacks.AccessCallback = null;

            this.SetHardwareBreakpoint(address, DEBUG_BREAKPOINT_ACCESS_TYPE.READ, size.ToUInt32());
        }

        public void FindWhatAccesses(UInt64 address, BreakpointSize size, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = null;
            this.EventCallBacks.ReadCallback = null;
            this.EventCallBacks.AccessCallback = callback;

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
                    this.BaseClient = DebugEngine.CreateIDebugClient();
                    this.Client.AddProcessOptions(DEBUG_PROCESS.DETACH_ON_EXIT);

                    this.EventCallBacks.BaseClient = this.BaseClient;

                    this.Client.SetOutputCallbacksWide(this.OutputCallBacks);
                    this.Client.SetEventCallbacksWide(this.EventCallBacks);

                    this.Client.AttachProcess(0, unchecked((UInt32)this.SystemProcess.Id), DEBUG_ATTACH.DEFAULT);
                    this.Control.WaitForEvent(DEBUG_WAIT.DEFAULT, 0);

                    /*
                    this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sx", DEBUG_EXECUTE.ECHO);

                    String[] exceptions = "ct et cpr epr ld ud ser ibp iml out av asrt aph bpe bpec eh clr clrn cce cc dm dbce gp ii ip dz iov ch hc lsq isc 3c svh sse ssec sbo sov vs vcpp wkd rto rtt wob wos *".Split(' ');

                    foreach (String exception in exceptions)
                    {
                        this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe -h " + exception, DEBUG_EXECUTE.ECHO);
                    }

                    String[] exceptions2 = "ssessec bpebpec ccecc".Split(' ');

                    foreach (String exception in exceptions2)
                    {
                        this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe -h " + exception, DEBUG_EXECUTE.ECHO);
                    }

                    this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe -h 80000003", DEBUG_EXECUTE.ECHO);
                    // int a = this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe wob", DEBUG_EXECUTE.ECHO);
                    // int b = this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe wos", DEBUG_EXECUTE.ECHO);
                    // int c = this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe 4000001e", DEBUG_EXECUTE.ECHO);
                    // int d = this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sxe 4000001f", DEBUG_EXECUTE.ECHO);
                    */

                    /*
                    List<DEBUG_EXCEPTION_FILTER_PARAMETERS> exceptionFilters = new List<DEBUG_EXCEPTION_FILTER_PARAMETERS>();

                    exceptionFilters.Add(new DEBUG_EXCEPTION_FILTER_PARAMETERS()
                    {
                        // WOW64 single step exception
                        ExceptionCode = 0x4000001e,
                        ExecutionOption = DEBUG_FILTER_EXEC_OPTION.BREAK,
                        ContinueOption = DEBUG_FILTER_CONTINUE_OPTION.GO_NOT_HANDLED,
                    });

                    exceptionFilters.Add(new DEBUG_EXCEPTION_FILTER_PARAMETERS()
                    {
                        // WOW64 breakpoint exception
                        ExceptionCode = 0x4000001f,
                        ExecutionOption = DEBUG_FILTER_EXEC_OPTION.BREAK,
                        ContinueOption = DEBUG_FILTER_CONTINUE_OPTION.GO_NOT_HANDLED,
                    });

                    exceptionFilters.Add(new DEBUG_EXCEPTION_FILTER_PARAMETERS()
                    {
                        ExceptionCode = 0x80000003,
                        ExecutionOption = DEBUG_FILTER_EXEC_OPTION.BREAK,
                        ContinueOption = DEBUG_FILTER_CONTINUE_OPTION.GO_NOT_HANDLED,
                    });

                    exceptionFilters.Add(new DEBUG_EXCEPTION_FILTER_PARAMETERS()
                    {
                        ExceptionCode = 0x80000004,
                        ExecutionOption = DEBUG_FILTER_EXEC_OPTION.BREAK,
                        ContinueOption = DEBUG_FILTER_CONTINUE_OPTION.GO_NOT_HANDLED,
                    });

                    this.Control.SetExceptionFilterParameters((UInt32)exceptionFilters.Count, exceptionFilters.ToArray());
                    */
                    this.Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "sx", DEBUG_EXECUTE.ECHO);

                    this.IsAttached = true;
                }
                catch (Exception ex)
                {
                    Output.Output.Log(Output.LogLevel.Error, "Error attaching debugger", ex);
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, this.Scheduler.ExclusiveScheduler).Wait();

            // Listen for events such as breakpoint hits
            this.ListenForEvents();
        }

        private void ListenForEvents()
        {
            Task.Run(() =>
            {
                while (this.IsAttached)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            DEBUG_STATUS status;

                            this.Control.GetExecutionStatus(out status);

                            if (status == DEBUG_STATUS.NO_DEBUGGEE)
                            {
                                return;
                            }

                            if (status == DEBUG_STATUS.GO || status == DEBUG_STATUS.BREAK || status == DEBUG_STATUS.STEP_BRANCH || status == DEBUG_STATUS.STEP_INTO || status == DEBUG_STATUS.STEP_OVER)
                            {
                                Int32 hr = this.Control.WaitForEvent(DEBUG_WAIT.DEFAULT, UInt32.MaxValue);
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Output.Output.Log(Output.LogLevel.Error, "Error listening for debugger events", ex);
                        }
                    }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, this.Scheduler.ExclusiveScheduler).Wait();
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

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Int32 hResult = this.Control.AddBreakpoint2(breakpointType, AnyId, out breakpoint);

                    if (hResult < 0)
                    {
                        throw new Exception("Invalid HRESULT: " + hResult.ToString());
                    }

                    breakpoint.SetOffset(address);
                    breakpoint.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
                    breakpoint.SetDataParameters(size, access);
                }
                catch (Exception ex)
                {
                    Output.Output.Log(Output.LogLevel.Error, "Error setting breakpoint", ex);
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, this.Scheduler.ExclusiveScheduler).Wait();

            return breakpoint;
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