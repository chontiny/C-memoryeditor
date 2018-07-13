namespace Squalr.Engine.HookServer.Random
{
    using EasyHook;
    using SqualrHookClient.Source;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Interface to a hook that controls speed in an external process.
    /// Credits to the forum post here for the performance counter hook: http://bbs.csdn.net/topics/390987111
    /// </summary>
    [Serializable]
    internal unsafe class RandomHook
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomHook" /> class.
        /// </summary>
        public RandomHook(HookClientBase hookClient)
        {
            this.HookClient = hookClient;

            return;

            this.HookClient.Log("Unrandomizer loaded");

            this.MsvcrtRandomHook = HookServer.CreateHook("msvcrt", "rand", new MsvcrtRandomDelegate(this.MsvcrtRandomEx), this);
            this.CryptGenRandomHook = HookServer.CreateHook("Advapi32.dll", "CryptGenRandom", new CryptGenRandomDelegate(this.CryptGenRandomEx), this);
            this.BcryptGenRandomHook = HookServer.CreateHook("Bcrypt", "BCryptGenRandom", new BcryptGenRandomDelegate(this.BCryptGenRandomEx), this);
        }

        /// <summary>
        /// Gets or sets the speedup of the external process.
        /// </summary>
        public Double SpeedUp { get; set; }

        private HookClientBase HookClient { get; set; }

        /// <summary>
        /// Gets or sets the hook to the GetTickCountHook method in the target process.
        /// </summary>
        private LocalHook GetTickCountHook { get; set; }

        /// <summary>
        /// Gets or sets the hook to the GetTickCount64 method in the target process.
        /// </summary>
        private LocalHook GetTickCount64Hook { get; set; }

        /// <summary>
        /// Gets or sets the hook to the msvcrt random method in the target process.
        /// </summary>
        private LocalHook MsvcrtRandomHook { get; set; }

        /// <summary>
        /// Gets or sets the hook to the msvcrt random method in the target process.
        /// </summary>
        private LocalHook CryptGenRandomHook { get; set; }

        /// <summary>
        /// Gets or sets the hook to the msvcrt random method in the target process.
        /// </summary>
        private LocalHook BcryptGenRandomHook { get; set; }

        public Int32 MsvcrtRandomEx()
        {
            return 0;
        }

        public Boolean CryptGenRandomEx(IntPtr hProv, UInt32 dwLen, Byte* pbBuffer)
        {
            Boolean result = CryptGenRandom(hProv, dwLen, pbBuffer);

            for (Int32 index = 0; index < dwLen; index++)
            {
                pbBuffer[index] = 0;
            }

            return result;
        }

        public Int32 BCryptGenRandomEx(IntPtr hAlgorithm, Byte[] pbBuffer, Int32 cbBuffer, Int32 dwFlags)
        {
            Int32 result = BCryptGenRandom(hAlgorithm, pbBuffer, cbBuffer, dwFlags);

            for (Int32 index = 0; index < cbBuffer; index++)
            {
                pbBuffer[index] = 0;
            }

            return result;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate Int32 MsvcrtRandomDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate Boolean CryptGenRandomDelegate(IntPtr hProv, UInt32 dwLen, Byte* pbBuffer);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate Int32 BcryptGenRandomDelegate(IntPtr hAlgorithm, Byte[] pbBuffer, Int32 cbBuffer, Int32 dwFlags);

        [DllImport("msvcrt")]
        private static extern Int32 rand();

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern Boolean CryptGenRandom(IntPtr hProv, UInt32 dwLen, Byte* pbBuffer);

        [DllImport("Bcrypt", CharSet = CharSet.Unicode)]
        private static extern Int32 BCryptGenRandom(IntPtr hAlgorithm, Byte[] pbBuffer, Int32 cbBuffer, Int32 dwFlags);
    }
    //// End class
}
//// End namespace