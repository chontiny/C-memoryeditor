using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;

namespace AnathenaProxy
{
    /// <summary>
    /// Proxy service to be contained by a 32 and 64 bit service, with services exposed via IPC. Useful for certain things that
    /// Anathena requires, such as:
    /// - FASM Compiler, which can only be run in 32 bit mode
    /// - Microsoft.Diagnostics.Runtime, which can only be used on processes of the same bitness
    /// </summary>
    public class AnathenaProxy
    {
        private const Int32 ParentCheckDelayMs = 500;

        public AnathenaProxy(Int32 parentProcessId, String pipeName, String waitEventName)
        {
            // Create an event to have the client wait until we are finished starting the service
            EventWaitHandle processStartingEvent = new EventWaitHandle(false, EventResetMode.ManualReset, waitEventName);

            InitializeAutoExit(parentProcessId);

            ServiceHost serviceHost = new ServiceHost(typeof(ProxyService));
            serviceHost.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            serviceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            serviceHost.AddServiceEndpoint(typeof(IProxyService), binding, pipeName);
            serviceHost.Open();

            processStartingEvent.Set();

            Console.WriteLine("Anathena proxy library loaded");
            Console.ReadLine();
        }

        public static Boolean IsRunning(Int32 parentProcessId)
        {
            try
            {
                Process.GetProcessById(parentProcessId);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private void InitializeAutoExit(Int32 parentProcessId)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (!IsRunning(parentProcessId))
                    {
                        break;
                    }

                    Thread.Sleep(AnathenaProxy.ParentCheckDelayMs);
                }

                Environment.Exit(0);
            });
        }
    }
    //// End class
}
//// End namespace