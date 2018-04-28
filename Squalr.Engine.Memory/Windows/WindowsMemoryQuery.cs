namespace Squalr.Engine.Memory.Windows
{
    using Native;
    using Processes;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Logging;
    using Squalr.Engine.Memory.Windows.PEB;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Utils;
    using Utils.Extensions;
    using static Native.Enumerations;
    using static Native.Structures;

    /// <summary>
    /// Class for memory editing a remote process.
    /// </summary>
    internal class WindowsMemoryQuery : IMemoryQuery
    {
        /// <summary>
        /// The chunk size for memory regions. Prevents large allocations.
        /// </summary>
        private const Int32 ChunkSize = 2000000000;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAdapter"/> class.
        /// </summary>
        public WindowsMemoryQuery()
        {
            // Subscribe to process events
            ProcessInfo.Default.Subscribe(this);
        }

        /// <summary>
        /// Gets a reference to the target process. This is an optimization to minimize accesses to the Processes component of the Engine.
        /// </summary>
        public Process ExternalProcess { get; set; }

        /// <summary>
        /// Recieves a process update. This is an optimization over grabbing the process from the <see cref="IProcessInfo"/> component
        /// of the <see cref="EngineCore"/> every time we need it, which would be cumbersome when doing hundreds of thousands of memory read/writes.
        /// </summary>
        /// <param name="process">The newly selected process.</param>
        public void Update(Process process)
        {
            this.ExternalProcess = process;
        }

        /// <summary>
        /// Gets the address of the stacks in the opened process.
        /// </summary>
        /// <returns>A pointer to the stacks of the opened process.</returns>
        public IEnumerable<NormalizedRegion> GetStackAddresses()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the address(es) of the heap in the target process.
        /// </summary>
        /// <returns>The heap addresses in the target process.</returns>
        public IEnumerable<NormalizedRegion> GetHeapAddresses()
        {
            ManagedPeb peb = new ManagedPeb(this.ExternalProcess == null ? IntPtr.Zero : this.ExternalProcess.Handle);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets regions of memory allocated in the remote process based on provided parameters.
        /// </summary>
        /// <param name="requiredProtection">Protection flags required to be present.</param>
        /// <param name="excludedProtection">Protection flags that must not be present.</param>
        /// <param name="allowedTypes">Memory types that can be present.</param>
        /// <param name="startAddress">The start address of the query range.</param>
        /// <param name="endAddress">The end address of the query range.</param>
        /// <returns>A collection of pointers to virtual pages in the target process.</returns>
        public IEnumerable<NormalizedRegion> GetVirtualPages(
            MemoryProtectionEnum requiredProtection,
            MemoryProtectionEnum excludedProtection,
            MemoryTypeEnum allowedTypes,
            UInt64 startAddress,
            UInt64 endAddress)
        {
            MemoryProtectionFlags requiredFlags = 0;
            MemoryProtectionFlags excludedFlags = 0;

            if ((requiredProtection & MemoryProtectionEnum.Write) != 0)
            {
                requiredFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                requiredFlags |= MemoryProtectionFlags.ReadWrite;
            }

            if ((requiredProtection & MemoryProtectionEnum.Execute) != 0)
            {
                requiredFlags |= MemoryProtectionFlags.Execute;
                requiredFlags |= MemoryProtectionFlags.ExecuteRead;
                requiredFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                requiredFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            if ((requiredProtection & MemoryProtectionEnum.CopyOnWrite) != 0)
            {
                requiredFlags |= MemoryProtectionFlags.WriteCopy;
                requiredFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            if ((excludedProtection & MemoryProtectionEnum.Write) != 0)
            {
                excludedFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                excludedFlags |= MemoryProtectionFlags.ReadWrite;
            }

            if ((excludedProtection & MemoryProtectionEnum.Execute) != 0)
            {
                excludedFlags |= MemoryProtectionFlags.Execute;
                excludedFlags |= MemoryProtectionFlags.ExecuteRead;
                excludedFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                excludedFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            if ((excludedProtection & MemoryProtectionEnum.CopyOnWrite) != 0)
            {
                excludedFlags |= MemoryProtectionFlags.WriteCopy;
                excludedFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            IEnumerable<MemoryBasicInformation64> memoryInfo = Memory.VirtualPages(this.ExternalProcess == null ? IntPtr.Zero : this.ExternalProcess.Handle, startAddress, endAddress, requiredFlags, excludedFlags, allowedTypes);

            IList<NormalizedRegion> regions = new List<NormalizedRegion>();

            foreach (MemoryBasicInformation64 next in memoryInfo)
            {

                if (next.RegionSize < ChunkSize)
                {
                    regions.Add(new NormalizedRegion(next.BaseAddress.ToUInt64(), next.RegionSize.ToInt32()));
                }
                else
                {
                    // This region requires chunking
                    Int64 remaining = next.RegionSize;
                    UInt64 currentBaseAddress = next.BaseAddress.ToUInt64();

                    while (remaining >= ChunkSize)
                    {
                        regions.Add(new NormalizedRegion(currentBaseAddress, ChunkSize));

                        remaining -= ChunkSize;
                        currentBaseAddress = currentBaseAddress.Add(ChunkSize, wrapAround: false);
                    }

                    if (remaining > 0)
                    {
                        regions.Add(new NormalizedRegion(currentBaseAddress, remaining.ToInt32()));
                    }
                }
            }

            return regions;
        }

        /// <summary>
        /// Gets all virtual pages in the opened process.
        /// </summary>
        /// <returns>A collection of regions in the process.</returns>
        public IEnumerable<NormalizedRegion> GetAllVirtualPages()
        {
            MemoryTypeEnum flags = MemoryTypeEnum.None | MemoryTypeEnum.Private | MemoryTypeEnum.Image | MemoryTypeEnum.Mapped;

            return this.GetVirtualPages(0, 0, flags, 0, this.GetMaximumAddress());
        }

        /// <summary>
        /// Gets the maximum address possible in the target process.
        /// </summary>
        /// <returns>The maximum address possible in the target process.</returns>
        public UInt64 GetMaximumAddress()
        {
            if (IntPtr.Size == Conversions.SizeOf(DataType.Int32))
            {
                return unchecked(UInt32.MaxValue);
            }
            else if (IntPtr.Size == Conversions.SizeOf(DataType.Int64))
            {
                return unchecked(UInt64.MaxValue);
            }

            throw new Exception("Unable to determine maximum address");
        }

        /// <summary>
        /// Gets the maximum usermode address possible in the target process.
        /// </summary>
        /// <returns>The maximum usermode address possible in the target process.</returns>
        public UInt64 GetMaxUsermodeAddress()
        {
            if (ProcessInfo.Default.IsOpenedProcess32Bit())
            {
                return Int32.MaxValue;
            }
            else
            {
                return 0x7FFFFFFFFFF;
            }
        }

        /// <summary>
        /// Gets all modules in the opened process.
        /// </summary>
        /// <returns>A collection of modules in the process.</returns>
        public IEnumerable<NormalizedModule> GetModules()
        {
            // Query all modules in the target process
            IntPtr[] modulePointers = new IntPtr[0];
            Int32 bytesNeeded = 0;

            List<NormalizedModule> modules = new List<NormalizedModule>();

            if (this.ExternalProcess == null)
            {
                return modules;
            }

            try
            {
                // Determine number of modules
                if (!NativeMethods.EnumProcessModulesEx(this.ExternalProcess.Handle, modulePointers, 0, out bytesNeeded, (UInt32)Enumerations.ModuleFilter.ListModulesAll))
                {
                    return modules;
                }

                Int32 totalNumberofModules = bytesNeeded / IntPtr.Size;
                modulePointers = new IntPtr[totalNumberofModules];

                if (NativeMethods.EnumProcessModulesEx(this.ExternalProcess.Handle, modulePointers, bytesNeeded, out bytesNeeded, (UInt32)Enumerations.ModuleFilter.ListModulesAll))
                {
                    for (Int32 index = 0; index < totalNumberofModules; index++)
                    {
                        StringBuilder moduleFilePath = new StringBuilder(1024);
                        NativeMethods.GetModuleFileNameEx(this.ExternalProcess.Handle, modulePointers[index], moduleFilePath, (UInt32)moduleFilePath.Capacity);

                        String moduleName = Path.GetFileName(moduleFilePath.ToString());
                        ModuleInformation moduleInformation = new ModuleInformation();
                        NativeMethods.GetModuleInformation(this.ExternalProcess.Handle, modulePointers[index], out moduleInformation, (UInt32)(IntPtr.Size * modulePointers.Length));

                        // Ignore modules in 64-bit address space for WoW64 processes
                        if (ProcessInfo.Default.IsOpenedProcess32Bit() && moduleInformation.ModuleBase.ToUInt64() > Int32.MaxValue)
                        {
                            continue;
                        }

                        // Convert to a normalized module and add it to our list
                        NormalizedModule module = new NormalizedModule(moduleName, moduleInformation.ModuleBase.ToUInt64(), (Int32)moduleInformation.SizeOfImage);
                        modules.Add(module);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to fetch modules from selected process", ex);
            }

            return modules;
        }
    }
    //// End class
}
//// End namespace