namespace Squalr.Source.Scanners.Pointers
{
    using ScanConstraints;
    using Snapshots;
    using Squalr.Source.ProjectExplorer;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Trace-Retrace Algorithm:
    /// 0) Potential pre-processing -- no idea how many valid pointers exist in a process, but we may be able to:
    ///     - Store all pointers to use
    ///     - Store all regions that contain a pointer
    ///  1) Start with a base address. Convert this to a range that spans 1024 in each direction, add this to the target list
    ///  2) REPEAT FOR N LEVELS:
    ///     - Search for all pointer values that fall in in the target list
    ///     - Convert these pointers to spanning regions, and add them to the target list, clearing the old list
    ///  3) Retrace pointers. We will not trace pointers with invalid bases. Loop from last level to first level:
    ///    - Compare pointer to all pointers in the previous level. Store offsets from current level to all pointers in previous level.
    /// </summary>
    internal class PointerScanner : ScheduledTask
    {
        private const Int32 MaxAdd = 4096;

        public PointerScanner(UInt64 targetAddress = 0x100579C) : base(
            taskName: "Pointer Scanner",
            isRepeated: false,
            trackProgress: true)
        {
            this.IndexValueMap = new ConcurrentDictionary<Int32, String>();
            this.PointerPool = new ConcurrentDictionary<IntPtr, IntPtr>();
            this.ConnectedPointers = new List<ConcurrentDictionary<IntPtr, IntPtr>>();
            this.ScanMode = ScanModeEnum.ReadValues;

            this.Dependencies.Enqueue(new PointerRetracer(targetAddress));
        }

        private enum ScanModeEnum
        {
            ReadValues,
            Scan,
            Rescan
        }

        private Snapshot Snapshot { get; set; }

        private ScanModeEnum ScanMode { get; set; }

        private Int32 StartReadIndex { get; set; }

        private Int32 EndReadIndex { get; set; }

        private ConcurrentDictionary<Int32, String> IndexValueMap { get; set; }

        private ConcurrentDictionary<IntPtr, IntPtr> PointerPool { get; set; }

        private List<ConcurrentDictionary<IntPtr, IntPtr>> ConnectedPointers { get; set; }

        private Snapshot AcceptedBases { get; set; }

        private ScanConstraintManager ScanConstraintManager { get; set; }

        private Boolean IsAddressMode { get; set; }

        private List<Tuple<IntPtr, List<Int32>>> AcceptedPointers { get; set; }

        private Type ElementType { get; set; }

        private IntPtr TargetAddress { get; set; }

        private Int32 MaxPointerLevel { get; set; }

        private Int32 MaxPointerOffset { get; set; }

        public void UpdateReadBounds(Int32 startReadIndex, Int32 endReadIndex)
        {
            this.StartReadIndex = startReadIndex;
            this.EndReadIndex = endReadIndex;
        }

        public void SetElementType(Type elementType)
        {
            this.ElementType = elementType;
        }

        public void SetRescanMode(Boolean isAddressMode)
        {
            this.IsAddressMode = isAddressMode;
        }

        public void SetTargetAddress(IntPtr address)
        {
            this.TargetAddress = address;
        }

        public void SetScanConstraintManager(ScanConstraintManager scanConstraintManager)
        {
            this.ScanConstraintManager = scanConstraintManager;
        }

        public void SetMaxPointerLevel(Int32 maxPointerLevel)
        {
            this.MaxPointerLevel = maxPointerLevel;
        }

        public void SetMaxPointerOffset(Int32 maxPointerOffset)
        {
            this.MaxPointerOffset = maxPointerOffset;
        }

        public void AddSelectionToTable(Int32 minIndex, Int32 maxIndex)
        {
            if (minIndex < 0)
            {
                minIndex = 0;
            }

            if (maxIndex > this.AcceptedPointers.Count)
            {
                maxIndex = this.AcceptedPointers.Count;
            }

            Int32 count = 0;

            for (Int32 index = minIndex; index <= maxIndex; index++)
            {
                String pointerValue = String.Empty;
                this.IndexValueMap.TryGetValue(index, out pointerValue);

                PointerItem newPointer = new PointerItem(
                    baseAddress: this.AcceptedPointers[index].Item1,
                    elementType: this.ElementType,
                    description: "New Pointer",
                    moduleName: String.Empty,
                    pointerOffsets: this.AcceptedPointers[index].Item2,
                    isValueHex: false,
                    value: pointerValue
                );

                ProjectExplorerViewModel.GetInstance().AddNewProjectItems(addToSelected: true, projectItems: newPointer);

                if (++count >= PointerScanner.MaxAdd)
                {
                    break;
                }
            }
        }

        public String GetValueAtIndex(Int32 index)
        {
            if (this.IndexValueMap.ContainsKey(index))
            {
                return this.IndexValueMap[index];
            }

            return "-";
        }

        public String GetAddressAtIndex(Int32 index)
        {
            return Conversions.ToHex(this.AcceptedPointers[index].Item1, formatAsAddress: true, includePrefix: false);
        }

        public IEnumerable<String> GetOffsetsAtIndex(Int32 index)
        {
            List<String> offsets = new List<String>();
            this.AcceptedPointers[index].Item2.ForEach(x => offsets.Add((x < 0 ? "-" : String.Empty) + Math.Abs(x).ToString("X")));
            return offsets;
        }

        public Int32 GetMaxPointerLevel()
        {
            return this.MaxPointerLevel;
        }

        public Int32 GetMaxPointerOffset()
        {
            return this.MaxPointerOffset;
        }

        public void BeginPointerScan()
        {
            this.ScanMode = ScanModeEnum.Scan;
        }

        public void BeginPointerRescan()
        {
            this.ScanMode = ScanModeEnum.Rescan;
        }

        protected override void OnBegin()
        {
        }

        /// <summary>
        /// Called when the scan updates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            return;

            // Scan mode determines the action to make, such that the action always happens on this task thread
            switch (this.ScanMode)
            {
                case ScanModeEnum.ReadValues:
                    break;
                case ScanModeEnum.Scan:
                    this.BuildPointerPool();
                    this.TracePointers();
                    this.ScanMode = ScanModeEnum.ReadValues;
                    break;
                case ScanModeEnum.Rescan:
                    this.PointerRescan();
                    this.ScanMode = ScanModeEnum.ReadValues;
                    break;
            }
        }

        protected override void OnEnd()
        {
        }

        private IntPtr ResolvePointer(Tuple<IntPtr, List<Int32>> fullPointer)
        {
            IntPtr pointer = fullPointer.Item1;
            List<Int32> offsets = fullPointer.Item2;

            if (offsets == null || offsets.Count == 0)
            {
                return pointer;
            }

            Boolean successReading = true;

            foreach (Int32 offset in offsets)
            {
                pointer = EngineCore.GetInstance().VirtualMemory.Read<IntPtr>(pointer, out successReading);
                pointer = pointer.Add(offset);

                if (!successReading)
                {
                    break;
                }
            }

            return pointer;
        }

        private void PointerRescan()
        {
            this.PrintDebugTag();

            if (this.IsAddressMode)
            {
                this.RescanAddresses();
            }
            else
            {
                this.RescanValues();
            }
        }

        private void RescanAddresses()
        {
            this.PrintDebugTag();

            List<Tuple<IntPtr, List<Int32>>> retainedPointers = new List<Tuple<IntPtr, List<Int32>>>();

            foreach (Tuple<IntPtr, List<Int32>> fullPointer in this.AcceptedPointers)
            {
                if (this.ResolvePointer(fullPointer) == this.TargetAddress)
                {
                    retainedPointers.Add(fullPointer);
                }
            }

            this.AcceptedPointers = retainedPointers;
        }

        private void RescanValues()
        {
            this.PrintDebugTag();
        }

        private void BuildPointerPool()
        {
            /*
            this.PrintDebugTag();

            // Clear current pointer pool
            this.PointerPool.Clear();

            // Collect memory regions
            this.Snapshot = null; // SnapshotManager.GetInstance().CollectSnapshot(useSettings: false, usePrefilter: false).Clone();
            throw new Exception("Fix this");

            // Set to type of a pointer
            if (EngineCore.GetInstance().Processes.IsOpenedProcess32Bit())
            {
                this.Snapshot.ElementType = typeof(UInt32);
            }
            else
            {
                this.Snapshot.ElementType = typeof(UInt64);
            }

            // As far as I can tell, no valid pointers will end up being less than 0x10000 (UInt16.MaxValue), nor higher than usermode space.
            dynamic invalidPointerMin = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit() ? (Int32)UInt16.MaxValue : (Int64)UInt16.MaxValue;
            dynamic invalidPointerMax = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit() ? Int32.MaxValue : Int64.MaxValue;

            // Enforce 4-byte alignment of pointers
            this.Snapshot.Alignment = sizeof(Int32);

            Parallel.ForEach(
                this.Snapshot.Cast<Object>(),
                SettingsViewModel.GetInstance().ParallelSettings,
                (regionObject) =>
            {
                SnapshotRegionDeprecating region = (SnapshotRegionDeprecating)regionObject;
                Boolean readSuccess;

                // Read the memory of this region
                region.ReadAllRegionMemory(out readSuccess, true);

                if (!readSuccess)
                {
                    return;
                }

                if (!region.HasValues())
                {
                    return;
                }

                foreach (SnapshotElementDeprecating element in region)
                {
                    if (element.LessThanValue(invalidPointerMin))
                    {
                        continue;
                    }

                    if (element.GreaterThanValue(invalidPointerMax))
                    {
                        continue;
                    }

                    // Enforce 4-byte alignment of destination
                    if (element.GetCurrentValue() % 4 != 0)
                    {
                        continue;
                    }

                    IntPtr addressValue = new IntPtr(element.GetCurrentValue());

                    // Check if it is possible that this pointer is valid, if so keep it
                    if (Snapshot.ContainsAddress(addressValue))
                    {
                        PointerPool[element.BaseAddress] = addressValue;
                    }
                }

                // Clear the saved values, we do not need them now
                region.SetCurrentValues(null);
            });
            */
        }

        private SnapshotRegion AddressToRegion(IntPtr address)
        {
            // new SnapshotRegionDeprecating<Null>(new NormalizedRegion(address.Subtract(this.MaxPointerOffset), this.MaxPointerOffset * 2));
            return null;
        }

        private void TracePointers()
        {
            this.PrintDebugTag();
        }
    }
    //// End class
}
//// End namespace