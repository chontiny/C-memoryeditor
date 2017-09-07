namespace SqualrHookServer.Source.Network
{
    using EasyHook;
    using SqualrHookClient.Source;
    using System;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable]
    internal unsafe class NetworkHook
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkHook" /> class.
        /// </summary>
        public NetworkHook(HookClientBase hookClient)
        {
            // Do not disable networking for now
            return;

            this.HookClient = hookClient;

            try
            {
                this.RecvHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "recv"), new DRecv(MyRecv), this);
                this.SendHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "send"), new DSend(MySend), this);
                // this.RecvFromHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "recvfrom"), new DRecvFrom(MyRecvFrom), this);
                // this.SendToHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "sendto"), new DSendTo(MySendTo), this);
                // this.WSARecvHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "WSARecv"), new DWSARecv(MyWsaRecv), this);
                // this.WSASendHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "WSASend"), new DWSASend(MyWsaSend), this);

                RecvHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                SendHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                // RecvFromHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                // SendToHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                // WSARecvHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                // WSASendHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

                RemoteHooking.WakeUpProcess();

                this.HookClient.Log("Networking Disabled in Process");
            }
            catch (Exception ex)
            {
                this.HookClient.Log("Error activating network hooks", ex.ToString());
            }
        }

        private HookClientBase HookClient { get; set; }

        private LocalHook RecvHook { get; set; }

        private LocalHook RecvFromHook { get; set; }

        private LocalHook SendHook { get; set; }

        private LocalHook SendToHook { get; set; }

        private LocalHook WSARecvHook { get; set; }

        private LocalHook WSASendHook { get; set; }

        public Int32 MyRecvFrom(IntPtr Socket, IntPtr buffer, Int32 length, SendDataFlags flags, ref SockAddr from, IntPtr fromlen)
        {
            // this.HookClient.Log("recvfrom");

            return 0;
            // return recvfrom(Socket, buffer, length, flags, ref from, IntPtr.Zero);
        }

        private Int32 MyRecv(IntPtr socket, IntPtr buffer, Int32 length, Int32 flags)
        {
            // this.HookClient.Log("recv");

            return 0;
            // return recv(socket, buffer, length, flags);
        }

        private Int32 MySend(IntPtr socket, IntPtr buffer, Int32 length, Int32 flags)
        {
            // this.HookClient.Log("send");

            return 0;
            // return send(socket, buffer, length, flags);
        }

        private Int32 MySendTo(IntPtr Socket, IntPtr buffer, Int32 length, SendDataFlags flags, ref SockAddr To, Int32 tomlen)
        {
            // this.HookClient.Log("sendto");

            return 0;
            // return sendto(Socket, buffer, length, flags, ref To, tomlen);
        }

        private Int32 MyWsaRecv(
            IntPtr socketHandle,
            WSABuffer* buffer,
            Int32 bufferCount,
            out Int32 bytesTransferred,
            ref SocketFlags socketFlags,
            NativeOverlapped* overlapped,
            IntPtr completionRoutine)
        {
            // this.HookClient.Log("WSARecv");

            bytesTransferred = 0;
            return 0;
            // return WSARecv(socketHandle, buffer, bufferCount, out bytesTransferred, ref socketFlags, overlapped, completionRoutine);
        }

        private SocketError MyWsaSend(
            IntPtr socket,
            IntPtr buffer,
            Int32 length,
            out IntPtr numberOfBytesSent,
            SocketFlags flags,
            IntPtr overlapped,
            IntPtr completionRoutine)
        {
            // this.HookClient.Log("WSASend");

            numberOfBytesSent = IntPtr.Zero;
            return 0;
            // return WSASend(socket, buffer, length, out numberOfBytesSent, flags, overlapped, completionRoutine);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate Int32 DRecv(IntPtr socket, IntPtr buffer, Int32 length, Int32 flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate Int32 DRecvFrom(IntPtr Socket, IntPtr buffer, Int32 length, SendDataFlags flags, ref SockAddr from, IntPtr fromlen);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate Int32 DSend(IntPtr socket, IntPtr buffer, Int32 length, Int32 flags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate Int32 DSendTo(IntPtr Socket, IntPtr buffer, Int32 length, SendDataFlags flags, ref SockAddr To, Int32 tomlen);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate Int32 DWSARecv(
            IntPtr socketHandle,
            WSABuffer* buffer,
            Int32 bufferCount,
            out Int32 bytesTransferred,
            ref SocketFlags socketFlags,
            NativeOverlapped* overlapped,
            IntPtr completionRoutine);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate SocketError DWSASend(
            IntPtr socket,
            IntPtr buffer,
            Int32 length,
            out IntPtr numberOfBytesSent,
            SocketFlags flags,
            IntPtr overlapped,
            IntPtr completionRoutine);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 recv(IntPtr socket, IntPtr buffer, Int32 length, Int32 flags);

        [DllImport("ws2_32.dll")]
        public static extern Int32 recvfrom(IntPtr Socket, IntPtr buffer, Int32 length, SendDataFlags flags, ref SockAddr from, IntPtr fromlen);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 send(IntPtr socket, IntPtr buffer, Int32 length, Int32 flags);

        [DllImport("ws2_32.dll")]
        public static extern Int32 sendto(IntPtr Socket, IntPtr buffer, Int32 length, SendDataFlags flags, ref SockAddr To, Int32 tomlen);

        [DllImport("ws2_32.dll", SetLastError = true)]
        internal static extern Int32 WSARecv(
            IntPtr socketHandle,
            WSABuffer* buffer,
            Int32 bufferCount,
            out Int32 bytesTransferred,
            ref SocketFlags socketFlags,
            NativeOverlapped* overlapped,
            IntPtr completionRoutine);

        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern SocketError WSASend(
            IntPtr socket,
            IntPtr buffer,
            Int32 length,
            out IntPtr numberOfBytesSent,
            SocketFlags flags,
            IntPtr overlapped,
            IntPtr completionRoutine);

        [StructLayout(LayoutKind.Sequential)]
        public struct WSABuffer
        {
            // Length of Buffer
            public Int32 len;

            // Pointer to Buffer
            public IntPtr buf;
        }

        [Flags]
        public enum SendDataFlags
        {
            /// <summary>
            /// </summary>
            None = 0,
            /// <summary>
            /// Specifies that the data should not be subject to routing. A Windows Sockets service provider can choose to ignore this flag.
            /// </summary>
            DontRoute = 1,
            /// <summary>
            /// Sends OOB data (stream-style socket such as SOCK_STREAM only).
            /// </summary>
            OOB = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SockAddr
        {
            public short Family;
            public ushort Port;
            public AddressIP4 IPAddress;
            private Int64 Zero;

            public SockAddr(short Family, ushort Port, AddressIP4 IP)
            { this.Family = Family; this.Port = Port; this.IPAddress = IP; this.Zero = 0; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AddressIP4
        {
            public byte a1;
            public byte a2;
            public byte a3;
            public byte a4;

            public static AddressIP4 Broadcast { get { return new AddressIP4(255, 255, 255, 255); } }
            public static AddressIP4 AnyAddress { get { return new AddressIP4(0, 0, 0, 0); } }
            public static AddressIP4 Loopback { get { return new AddressIP4(127, 0, 0, 1); } }

            public AddressIP4(byte a1, byte a2, byte a3, byte a4)
            {
                this.a1 = a1;
                this.a2 = a2;
                this.a3 = a3;
                this.a4 = a4;
            }
        }
    }
    //// End class
}
//// End namespace