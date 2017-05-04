namespace SqualrHookServer.Source.Network
{
    using EasyHook;
    using SqualrHookClient.Source;
    using System;
    using System.Runtime.InteropServices;

    internal class NetworkHook
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkHook" /> class.
        /// </summary>
        public NetworkHook(IHookClient hookClient)
        {
            this.HookClient = hookClient;

            try
            {

                this.RecvHook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "recv"), new DRecv(MyRecv), this);
                this.SendHook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "send"), new DSend(MySend), this);

                RemoteHooking.WakeUpProcess();

                this.HookClient.Log("haha, nice");
            }
            catch (Exception ex)
            {
            }
        }

        private IHookClient HookClient { get; set; }

        private LocalHook RecvHook { get; set; }

        private LocalHook SendHook { get; set; }

        private int MyRecv(IntPtr socket, IntPtr buffer, int length, int flags)
        {
            int bytesCount = recv(socket, buffer, length, flags);

            this.HookClient.Log("recv");

            if (bytesCount > 0)
            {
                byte[] RecvBuffer = new byte[bytesCount];
                Marshal.Copy(buffer, RecvBuffer, 0, RecvBuffer.Length);
            }
            return bytesCount;
        }

        private Int32 MySend(IntPtr socket, IntPtr buffer, int length, int flags)
        {
            int bytesCount = send(socket, buffer, length, flags);

            this.HookClient.Log("send");

            if (bytesCount > 0)
            {
                byte[] RecvBuffer = new byte[bytesCount];
                Marshal.Copy(buffer, RecvBuffer, 0, RecvBuffer.Length);
            }
            return bytesCount;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int DRecv(IntPtr socket, IntPtr buffer, int length, int flags);

        [DllImport("WS2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int recv(IntPtr socket, IntPtr buffer, int length, int flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int DSend(IntPtr socket, IntPtr buffer, int length, int flags);

        [DllImport("WS2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern int send(IntPtr socket, IntPtr buffer, int length, int flags);
    }
    //// End class
}
//// End namespace