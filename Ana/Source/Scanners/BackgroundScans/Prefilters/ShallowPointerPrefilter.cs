namespace Ana.Source.Scanners.BackgroundScans.Prefilters
{
    using Engine;
    using Engine.OperatingSystems;
    using Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UserSettings;
    using Utils;
    using Utils.DataStructures;
    using Utils.Extensions;

    /// <summary>
    /// Class to collect all pointers in the module base of the target process, and slowly grow from there.
    /// The growth radius will be small, so only a small subset of the processes memory should be explored
    /// </summary>
    internal class ShallowPointerPrefilter : RepeatedTask, ISnapshotPrefilter
    {
        private const Int32 PointerRadius = 2048;

        private const Int32 RegionLimit = 8192;

        private const Int32 RescanTime = 4096;

        private const Int32 CompletionThreshold = 97;

        /// <summary>
        /// Singleton instance of the <see cref="ShallowPointerPrefilter"/> class
        /// </summary>
        private static Lazy<ISnapshotPrefilter> snapshotPrefilterInstance = new Lazy<ISnapshotPrefilter>(
            () => { return new ShallowPointerPrefilter(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets or sets the number of regions processed by this prefilter
        /// </summary>
        private Int64 processedCount;

        /// <summary>
        /// Prevents a default instance of the <see cref="ShallowPointerPrefilter" /> class from being created
        /// </summary>
        private ShallowPointerPrefilter()
        {
            this.PrefilteredSnapshot = new Snapshot<Null>();
            this.RegionLock = new Object();
            this.processedCount = 0;

            this.PrefilteredSnapshot.Alignment = SettingsViewModel.GetInstance().Alignment;
        }

        /// <summary>
        /// Gets or sets a lock for accessing snapshot regions
        /// </summary>
        private Object RegionLock { get; set; }

        /// <summary>
        /// Gets or sets the snapshot constructed by this prefilter
        /// </summary>
        private Snapshot<Null> PrefilteredSnapshot { get; set; }

        public static ISnapshotPrefilter GetInstance()
        {
            return ShallowPointerPrefilter.snapshotPrefilterInstance.Value;
        }

        public void BeginPrefilter()
        {
            this.Begin();
        }

        public Snapshot GetPrefilteredSnapshot()
        {
            lock (this.RegionLock)
            {
                return this.PrefilteredSnapshot.Clone();
            }
        }

        public override void Begin()
        {
            this.UpdateInterval = RescanTime;
            base.Begin();
        }

        protected override void OnUpdate()
        {
            this.ProcessPages();
            this.UpdateProgress();
        }

        /// <summary>
        /// Called when the repeated task completes
        /// </summary>
        protected override void OnEnd()
        {
            base.OnEnd();
        }

        /// <summary>
        /// Processes all pages, computing checksums to determine chunks of virtual pages that have changed
        /// </summary>
        private void ProcessPages()
        {
            Snapshot snapshot = SnapshotManager.GetInstance().CollectSnapshot(useSettings: false, usePrefilter: false).Clone();
            Boolean isOpenedProcess32Bit = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit();
            dynamic invalidPointerMin = isOpenedProcess32Bit ? (UInt32)UInt16.MaxValue : (UInt64)UInt16.MaxValue;
            dynamic invalidPointerMax = isOpenedProcess32Bit ? Int32.MaxValue : Int64.MaxValue;
            ConcurrentHashSet<IntPtr> foundPointers = new ConcurrentHashSet<IntPtr>();

            // Add static bases
            List<SnapshotRegion<Null>> baseRegions = new List<SnapshotRegion<Null>>();
            foreach (NormalizedModule normalizedModule in EngineCore.GetInstance().OperatingSystemAdapter.GetModules())
            {
                baseRegions.Add(new SnapshotRegion<Null>(normalizedModule.BaseAddress, normalizedModule.RegionSize));
            }

            this.PrefilteredSnapshot.AddSnapshotRegions(baseRegions);

            lock (this.RegionLock)
            {
                List<SnapshotRegion<Null>> pointerRegions = new List<SnapshotRegion<Null>>();

                foreach (IntPtr pointer in PointerCollector.GetInstance().GetFoundPointers())
                {
                    pointerRegions.Add(new SnapshotRegion<Null>(pointer.Subtract(ShallowPointerPrefilter.PointerRadius), ShallowPointerPrefilter.PointerRadius * 2));
                }

                this.PrefilteredSnapshot.AddSnapshotRegions(pointerRegions);
                this.processedCount = Math.Max(this.processedCount, this.PrefilteredSnapshot.GetRegionCount());
            }
        }

        private void UpdateProgress()
        {
            Int32 regionCount = 1;

            lock (this.RegionLock)
            {
                if (this.PrefilteredSnapshot != null)
                {
                    regionCount = Math.Max(regionCount, this.PrefilteredSnapshot.GetRegionCount());
                }
            }
        }
    }
    //// End class
}
//// End namespace