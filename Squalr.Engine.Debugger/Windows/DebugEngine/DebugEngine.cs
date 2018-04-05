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
            this.CancellationTokenSource = new CancellationTokenSource();

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

        private CancellationTokenSource CancellationTokenSource { get; set; }

        public void Update(NormalizedProcess process)
        {
            if (process == null)
            {
                return;
            }

            this.SystemProcess = Process.GetProcessById(process.ProcessId);
        }

        public void FindWhatWrites(UInt64 address, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = callback;
            this.EventCallBacks.ReadsCallback = null;
            this.EventCallBacks.AccessesCallback = null;

            // this.SetHardwareBreakpoint(address);
        }

        public void FindWhatReads(UInt64 address, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = null;
            this.EventCallBacks.ReadsCallback = callback;
            this.EventCallBacks.AccessesCallback = null;

            //  this.SetHardwareBreakpoint(address);
        }

        public void FindWhatAccesses(UInt64 address, MemoryAccessCallback callback)
        {
            this.Attach();

            this.EventCallBacks.WriteCallback = null;
            this.EventCallBacks.ReadsCallback = null;
            this.EventCallBacks.AccessesCallback = callback;

            //  this.SetHardwareBreakpoint(address);
        }

        public void Attach()
        {
            // Exit if already attached, or debug request fails
            if (this.IsAttached || (this.DebugRequestCallback != null && this.DebugRequestCallback()))
            {
                return;
            }

            bool initialized = false;

            Task.Run(() =>
            {
                try
                {
                    IDebugClient baseClient = DebugEngine.CreateIDebugClient();

                    this.Client = baseClient as IDebugClient6;
                    this.Control = baseClient as IDebugControl6;
                    // this.Control.AddEngineOptions(DEBUG_ENGOPT.INITIAL_BREAK);

                    OutputCallBacks outputCallBacks = new OutputCallBacks();
                    EventCallBacks eventCallBacks = new EventCallBacks();

                    eventCallBacks.Control = this.Control;

                    this.Client.SetOutputCallbacksWide(outputCallBacks);
                    this.Client.SetEventCallbacksWide(eventCallBacks);

                    this.Client.AttachProcess(0, unchecked((UInt32)this.SystemProcess.Id), DEBUG_ATTACH.DEFAULT);

                    while (true)
                    {
                        this.Control.WaitForEvent(0, UInt32.MaxValue);

                        if (!initialized)
                        {
                            this.SetSoftwareBreakpoint(0x1002ff5);
                            //   this.SetHardwareBreakpoint(0x100579c);
                        }

                        initialized = true;
                    }
                }
                catch (Exception ex)
                {
                    Output.Output.Log(Output.LogLevel.Error, "Error attaching debugger", ex);
                    return null;
                }
                finally
                {
                    initialized = true;
                }
            });

            while (!initialized)
            {
            }
        }

        private IDebugBreakpoint2 SetSoftwareBreakpoint(UInt64 address)
        {
            return this.SetBreakpoint(address, DEBUG_BREAKPOINT_TYPE.CODE);
        }

        private IDebugBreakpoint2 SetHardwareBreakpoint(UInt64 address)
        {
            return this.SetBreakpoint(address, DEBUG_BREAKPOINT_TYPE.DATA);
        }

        private IDebugBreakpoint2 SetBreakpoint(UInt64 address, DEBUG_BREAKPOINT_TYPE breakpointType)
        {
            const UInt32 AnyId = UInt32.MaxValue;

            try
            {
                IDebugBreakpoint2 breakpoint;

                this.Control.AddBreakpoint2(breakpointType, AnyId, out breakpoint);

                breakpoint.SetOffset(address);
                breakpoint.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);

                return breakpoint;
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Error, "Error setting breakpoint", ex);
                return null;
            }
        }

        private static IDebugClient CreateIDebugClient()
        {
            Guid guid = typeof(IDebugClient).GUID;
            DebugEngine.DebugCreate(ref guid, out object obj);

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
        internal static extern uint DebugCreate(ref Guid InterfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object Interface);
    }
    //// End class
}
//// End namespace