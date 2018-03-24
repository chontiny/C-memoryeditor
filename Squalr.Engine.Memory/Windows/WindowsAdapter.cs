namespace Squalr.Engine.Memory.Windows
{
    using Native;
    using Output;
    using Processes;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Memory.Windows.PEB;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Utils;
    using Utils.Extensions;
    using static Native.Enumerations;
    using static Native.Structures;

    /// <summary>
    /// Class for memory editing a remote process.
    /// </summary>
    internal class WindowsAdapter : IVirtualMemoryAdapter
    {
        /// <summary>
        /// A reference to target process.
        /// </summary>
        private Process systemProcess;

        /// <summary>
        /// The chunk size for memory regions. Prevents large allocations.
        /// </summary>
        private const Int32 ChunkSize = 2000000000;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAdapter"/> class.
        /// </summary>
        public WindowsAdapter()
        {
            // Subscribe to process events
            ProcessAdapterFactory.GetProcessAdapter().Subscribe(this);
        }

        /// <summary>
        /// Gets a reference to the target process. This is an optimization to minimize accesses to the Processes component of the Engine.
        /// </summary>
        public Process SystemProcess
        {
            get
            {
                try
                {
                    if (this.systemProcess?.HasExited == true)
                    {
                        this.systemProcess = null;
                    }
                }
                catch
                {
                }

                return this.systemProcess;
            }

            private set
            {
                this.systemProcess = value;
            }
        }

        /// <summary>
        /// Recieves a process update. This is an optimization over grabbing the process from the <see cref="IProcessAdapter"/> component
        /// of the <see cref="EngineCore"/> every time we need it, which would be cumbersome when doing hundreds of thousands of memory read/writes.
        /// </summary>
        /// <param name="process">The newly selected process.</param>
        public void Update(NormalizedProcess process)
        {
            if (process == null)
            {
                // Avoid setter functions
                this.systemProcess = null;
                return;
            }

            try
            {
                this.SystemProcess = Process.GetProcessById(process.ProcessId);
            }
            catch
            {
                // Avoid setter functions
                this.systemProcess = null;
            }
        }

        #region Read
        /// <summary>
        /// Reads the value of a specified type in the remote process.
        /// </summary>
        /// <param name="dataType">Type of value being read.</param>
        /// <param name="address">The address where the value is read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>A value.</returns>
        public Object Read(DataType dataType, IntPtr address, out Boolean success)
        {
            Object value;

            switch (dataType)
            {
                case DataType type when type == DataType.Byte:
                    value = this.Read<Byte>(address, out success);
                    break;
                case DataType type when type == DataType.SByte:
                    value = this.Read<SByte>(address, out success);
                    break;
                case DataType type when type == DataType.Int16:
                    value = this.Read<Int16>(address, out success);
                    break;
                case DataType type when type == DataType.Int32:
                    value = this.Read<Int32>(address, out success);
                    break;
                case DataType type when type == DataType.Int64:
                    value = this.Read<Int64>(address, out success);
                    break;
                case DataType type when type == DataType.UInt16:
                    value = this.Read<UInt16>(address, out success);
                    break;
                case DataType type when type == DataType.UInt32:
                    value = this.Read<UInt32>(address, out success);
                    break;
                case DataType type when type == DataType.UInt64:
                    value = this.Read<UInt64>(address, out success);
                    break;
                case DataType type when type == DataType.Single:
                    value = this.Read<Single>(address, out success);
                    break;
                case DataType type when type == DataType.Double:
                    value = this.Read<Double>(address, out success);
                    break;
                default:
                    value = "?";
                    success = false;
                    break;
            }

            if (!success)
            {
                value = "?";
            }

            return value;
        }

        /// <summary>
        /// Reads the value of a specified type in the remote process.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="address">The address where the value is read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>A value.</returns>
        public T Read<T>(IntPtr address, out Boolean success)
        {
            Byte[] byteArray = this.ReadBytes(address, Conversions.SizeOf(typeof(T)), out success);
            return Conversions.BytesToObject<T>(byteArray);
        }

        /// <summary>
        /// Reads an array of bytes in the remote process.
        /// </summary>
        /// <param name="address">The address where the array is read.</param>
        /// <param name="count">The number of cells.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>The array of bytes.</returns>
        public Byte[] ReadBytes(IntPtr address, Int32 count, out Boolean success)
        {
            return Memory.ReadBytes(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, address, count, out success);
        }

        /// <summary>
        /// Reads a string with a specified encoding in the remote process.
        /// </summary>
        /// <param name="address">The address where the string is read.</param>
        /// <param name="encoding">The encoding used.</param>
        /// <param name="success">Whether or not the read succeeded</param>
        /// <param name="maxLength">[Optional] The number of maximum bytes to read. The string is automatically cropped at this end ('\0' char).</param>
        /// <returns>The string.</returns>
        public String ReadString(IntPtr address, Encoding encoding, out Boolean success, Int32 maxLength = 512)
        {
            // Read the string
            String data = encoding.GetString(this.ReadBytes(address, maxLength, out success));

            // Search the end of the string
            Int32 end = data.IndexOf('\0');

            // Crop the string with this end
            return data.Substring(0, end);
        }

        #endregion

        #region Write
        /// <summary>
        /// Writes a value to memory in the opened process.
        /// </summary>
        /// <param name="elementType">The data type to write.</param>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The value to write.</param>
        public void Write(DataType elementType, IntPtr address, Object value)
        {
            Byte[] bytes;

            switch (elementType)
            {
                case DataType type when type == DataType.Byte || type == typeof(Boolean):
                    bytes = BitConverter.GetBytes((Byte)value);
                    break;
                case DataType type when type == DataType.SByte:
                    bytes = BitConverter.GetBytes((SByte)value);
                    break;
                case DataType type when type == DataType.Char:
                    bytes = Encoding.UTF8.GetBytes(new Char[] { (Char)value });
                    break;
                case DataType type when type == DataType.Int16:
                    bytes = BitConverter.GetBytes((Int16)value);
                    break;
                case DataType type when type == DataType.Int32:
                    bytes = BitConverter.GetBytes((Int32)value);
                    break;
                case DataType type when type == DataType.Int64:
                    bytes = BitConverter.GetBytes((Int64)value);
                    break;
                case DataType type when type == DataType.UInt16:
                    bytes = BitConverter.GetBytes((UInt16)value);
                    break;
                case DataType type when type == DataType.UInt32:
                    bytes = BitConverter.GetBytes((UInt32)value);
                    break;
                case DataType type when type == DataType.UInt64:
                    bytes = BitConverter.GetBytes((UInt64)value);
                    break;
                case DataType type when type == DataType.Single:
                    bytes = BitConverter.GetBytes((Single)value);
                    break;
                case DataType type when type == DataType.Double:
                    bytes = BitConverter.GetBytes((Double)value);
                    break;
                default:
                    throw new ArgumentException("Invalid type provided");
            }

            this.WriteBytes(address, bytes);
        }

        /// <summary>
        /// Writes the values of a specified type in the remote process.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="address">The address where the value is written.</param>
        /// <param name="value">The value to write.</param>
        public void Write<T>(IntPtr address, T value)
        {
            this.Write(typeof(T), address, (Object)value);
        }

        /// <summary>
        /// Write an array of bytes in the remote process.
        /// </summary>
        /// <param name="address">The address where the array is written.</param>
        /// <param name="byteArray">The array of bytes to write.</param>
        public void WriteBytes(IntPtr address, Byte[] byteArray)
        {
            // Write the byte array
            Memory.WriteBytes(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, address, byteArray);
        }

        /// <summary>
        /// Writes a string with a specified encoding in the remote process.
        /// </summary>
        /// <param name="address">The address where the string is written.</param>
        /// <param name="text">The text to write.</param>
        /// <param name="encoding">The encoding used.</param>
        public void WriteString(IntPtr address, String text, Encoding encoding)
        {
            // Write the text
            this.WriteBytes(address, encoding.GetBytes(text + '\0'));
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
            ManagedPeb peb = new ManagedPeb(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle);

            return null;
        }

        /// <summary>
        /// Allocates memory in the opened process.
        /// </summary>
        /// <param name="size">The size of the memory allocation.</param>
        /// <returns>A pointer to the location of the allocated memory.</returns>
        public IntPtr AllocateMemory(Int32 size)
        {
            return Memory.Allocate(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, IntPtr.Zero, size);
        }

        /// <summary>
        /// Allocates memory in the opened process.
        /// </summary>
        /// <param name="size">The size of the memory allocation.</param>
        /// <param name="allocAddress">The rough address of where the allocation should take place.</param>
        /// <returns>A pointer to the location of the allocated memory.</returns>
        public IntPtr AllocateMemory(Int32 size, IntPtr allocAddress)
        {
            return Memory.Allocate(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, allocAddress, size);
        }

        /// <summary>
        /// Deallocates memory in the opened process.
        /// </summary>
        /// <param name="address">The address to perform the region wide deallocation.</param>
        public void DeallocateMemory(IntPtr address)
        {
            Memory.Free(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, address);
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
            IntPtr startAddress,
            IntPtr endAddress)
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

            IEnumerable<MemoryBasicInformation64> memoryInfo = Memory.VirtualPages(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, startAddress, endAddress, requiredFlags, excludedFlags, allowedTypes);

            IList<NormalizedRegion> regions = new List<NormalizedRegion>();

            foreach (MemoryBasicInformation64 next in memoryInfo)
            {

                if (next.RegionSize < ChunkSize)
                {
                    regions.Add(new NormalizedRegion(next.BaseAddress, next.RegionSize.ToInt32()));
                }
                else
                {
                    // This region requires chunking
                    Int64 remaining = next.RegionSize;
                    IntPtr currentBaseAddress = next.BaseAddress;

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
            return this.GetVirtualPages(0, 0, flags, IntPtr.Zero, this.GetMaximumAddress());
        }

        /// <summary>
        /// Gets the maximum address possible in the target process.
        /// </summary>
        /// <returns>The maximum address possible in the target process.</returns>
        public IntPtr GetMaximumAddress()
        {
            if (IntPtr.Size == Conversions.SizeOf(DataType.Int32))
            {
                return unchecked(UInt32.MaxValue.ToIntPtr());
            }
            else if (IntPtr.Size == Conversions.SizeOf(DataType.Int64))
            {
                return unchecked(UInt64.MaxValue.ToIntPtr());
            }

            throw new Exception("Unable to determine maximum address");
        }

        /// <summary>
        /// Gets the maximum usermode address possible in the target process.
        /// </summary>
        /// <returns>The maximum usermode address possible in the target process.</returns>
        public UInt64 GetMaxUsermodeAddress()
        {
            if (ProcessAdapterFactory.GetProcessAdapter().IsOpenedProcess32Bit())
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

            if (this.SystemProcess == null)
            {
                return modules;
            }

            try
            {
                // Determine number of modules
                if (!NativeMethods.EnumProcessModulesEx(this.SystemProcess.Handle, modulePointers, 0, out bytesNeeded, (UInt32)Enumerations.ModuleFilter.ListModulesAll))
                {
                    return modules;
                }

                Int32 totalNumberofModules = bytesNeeded / IntPtr.Size;
                modulePointers = new IntPtr[totalNumberofModules];

                if (NativeMethods.EnumProcessModulesEx(this.SystemProcess.Handle, modulePointers, bytesNeeded, out bytesNeeded, (UInt32)Enumerations.ModuleFilter.ListModulesAll))
                {
                    for (Int32 index = 0; index < totalNumberofModules; index++)
                    {
                        StringBuilder moduleFilePath = new StringBuilder(1024);
                        NativeMethods.GetModuleFileNameEx(this.SystemProcess.Handle, modulePointers[index], moduleFilePath, (UInt32)moduleFilePath.Capacity);

                        String moduleName = Path.GetFileName(moduleFilePath.ToString());
                        ModuleInformation moduleInformation = new ModuleInformation();
                        NativeMethods.GetModuleInformation(this.SystemProcess.Handle, modulePointers[index], out moduleInformation, (UInt32)(IntPtr.Size * modulePointers.Length));

                        // Ignore modules in 64-bit address space for WoW64 processes
                        if (ProcessAdapterFactory.GetProcessAdapter().IsOpenedProcess32Bit() && moduleInformation.ModuleBase.ToUInt64() > Int32.MaxValue)
                        {
                            continue;
                        }

                        // Convert to a normalized module and add it to our list
                        NormalizedModule module = new NormalizedModule(moduleName, moduleInformation.ModuleBase, (Int32)moduleInformation.SizeOfImage);
                        modules.Add(module);
                    }
                }
            }
            catch (Exception ex)
            {
                Output.Log(LogLevel.Error, "Unable to fetch modules from selected process", ex);
            }

            return modules;
        }

        /// <summary>
        /// Searches for an array of bytes in the opened process.
        /// </summary>
        /// <param name="bytes">The byte array to search for.</param>
        /// <returns>The address of the first match.</returns>
        public IntPtr SearchAob(Byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for an array of bytes in the opened process.
        /// </summary>
        /// <param name="pattern">The string pattern to search for.</param>
        /// <returns>The address of the first match.</returns>
        public IntPtr SearchAob(String pattern)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for an array of bytes in the opened process.
        /// </summary>
        /// <param name="pattern">The string pattern to search for.</param>
        /// <returns>The address of all matches.</returns>
        public IEnumerable<IntPtr> SearchllAob(String pattern)
        {
            throw new NotImplementedException();
        }

        public IntPtr EvaluatePointer(IntPtr address, IEnumerable<Int32> offsets)
        {
            IntPtr finalAddress = address;

            if (!offsets.IsNullOrEmpty())
            {
                // Add and trace offsets
                foreach (Int32 offset in offsets.Take(offsets.Count() - 1))
                {
                    Boolean success;
                    if (ProcessAdapterFactory.GetProcessAdapter().IsOpenedProcess32Bit())
                    {
                        finalAddress = (this.Read<UInt32>(finalAddress + offset, out success).ToInt64()).ToIntPtr();
                    }
                    else
                    {
                        finalAddress = (this.Read<UInt64>(finalAddress, out success).ToInt64() + offset).ToIntPtr();
                    }
                }

                // The last offset is added, but not traced
                finalAddress = finalAddress.Add(offsets.Last());
            }

            return finalAddress;
        }

        #endregion
    }
    //// End class
}
//// End namespace