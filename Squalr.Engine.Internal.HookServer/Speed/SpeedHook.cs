namespace Squalr.Engine.HookServer.Speed
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
    internal class SpeedHook
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedHook" /> class.
        /// </summary>
        public SpeedHook(HookClientBase hookClient)
        {
            this.HookClient = hookClient;

            return;

            this.HookClient.Log("Speedhack loaded");

            this.SpeedUp = 3.0;

            try
            {
                this.QueryPerformanceCounterHook = HookServer.CreateHook("Kernel32.dll", "QueryPerformanceCounter", new QueryPerformanceCounterDelegate(this.QueryPerformanceCounterEx), this);
                this.GetTickCountHook = HookServer.CreateHook("Kernel32.dll", "GetTickCount", new GetTickCountDelegate(this.GetTickCountEx), this);
                this.GetTickCount64Hook = HookServer.CreateHook("Kernel32.dll", "GetTickCount64", new GetTickCount64Delegate(this.GetTickCount64Ex), this);
            }
            catch (Exception ex)
            {
                this.HookClient.Log("Error activating speed hooks", ex.ToString());
            }
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
        /// Gets or sets the hook to the QueryPerformanceCounter method in the target process.
        /// </summary>
        private LocalHook QueryPerformanceCounterHook { get; set; }

        private Int64 StoredPerformanceCounterRealTime { get; set; }

        private Int64 StoredPerformanceCounterFakeTime { get; set; }

        private UInt32 StoredTickCountRealTime { get; set; }

        private UInt32 StoredTickCountFakeTime { get; set; }

        private UInt64 StoredTickCount64RealTime { get; set; }

        private UInt64 StoredTickCount64FakeTime { get; set; }

        private Boolean QueryPerformanceCounterEx(out Int64 fakeTime)
        {
            Boolean result;
            Int64 realTime = 0;
            Int32 tickCount = SpeedHook.timeGetTime();

            result = SpeedHook.QueryPerformanceCounter(out realTime);

            // Initialize
            if (StoredPerformanceCounterRealTime == 0)
            {
                this.StoredPerformanceCounterRealTime = realTime;
                this.StoredPerformanceCounterFakeTime = tickCount;
            }

            fakeTime = this.StoredPerformanceCounterFakeTime + (Int64)((realTime - this.StoredPerformanceCounterRealTime) * SpeedUp);

            this.StoredPerformanceCounterRealTime = realTime;
            this.StoredPerformanceCounterFakeTime = fakeTime;

            return result;
        }

        private UInt32 GetTickCountEx()
        {
            UInt32 realTime = SpeedHook.GetTickCount();
            UInt32 tickCount = unchecked((UInt32)(SpeedHook.timeGetTime()));

            // Initialize
            if (StoredTickCountRealTime == 0)
            {
                this.StoredTickCountRealTime = realTime;
                this.StoredTickCountFakeTime = tickCount;
            }

            UInt32 fakeTime = this.StoredTickCountFakeTime + (UInt32)((realTime - this.StoredTickCountRealTime) * SpeedUp);

            this.StoredTickCountRealTime = realTime;
            this.StoredTickCountFakeTime = fakeTime;

            return realTime;
        }

        private UInt64 GetTickCount64Ex()
        {
            UInt64 realTime = SpeedHook.GetTickCount();
            UInt64 tickCount = unchecked((UInt64)(SpeedHook.timeGetTime()));

            // Initialize
            if (StoredTickCount64RealTime == 0)
            {
                this.StoredTickCount64RealTime = realTime;
                this.StoredTickCount64FakeTime = tickCount;
            }

            UInt64 fakeTime = this.StoredTickCount64FakeTime + (UInt64)((realTime - this.StoredTickCount64RealTime) * SpeedUp);

            this.StoredTickCount64RealTime = realTime;
            this.StoredTickCount64FakeTime = fakeTime;

            return realTime;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate Boolean QueryPerformanceCounterDelegate(out Int64 lpPerformanceCount);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate UInt32 GetTickCountDelegate();

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        private delegate UInt64 GetTickCount64Delegate();

        [DllImport("Winmm.dll")]
        private static extern Int32 timeGetTime();

        [DllImport("kernel32.dll")]
        private static extern Boolean QueryPerformanceCounter(out Int64 lpPerformanceCount);

        [DllImport("kernel32.dll")]
        private static extern UInt32 GetTickCount();

        [DllImport("kernel32.dll")]
        private static extern UInt64 GetTickCount64();
    }
    //// End class
}
//// End namespace