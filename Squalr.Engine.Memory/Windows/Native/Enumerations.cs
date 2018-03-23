namespace Squalr.Engine.Memory.Windows.Native
{
    using System;

    /// <summary>
    /// Class containing native Windows enumerations.
    /// </summary>
    internal class Enumerations
    {
        /// <summary>
        /// The type of process information to be retrieved.
        /// </summary>
        public enum ProcessInformationClass
        {
            /// <summary>
            /// Retrieves a pointer to a PEB structure that can be used to determine whether the specified process is being debugged, 
            /// and a unique value used by the system to identify the specified process. 
            /// </summary>
            ProcessBasicInformation = 0x0,

            /// <summary>
            /// Retrieves a DWORD_PTR value that is the port number of the debugger for the process. 
            /// A nonzero value indicates that the process is being run under the control of a ring 3 debugger.
            /// </summary>
            ProcessDebugPort = 0x7,

            /// <summary>
            /// Determines whether the process is running in the WOW64 environment (WOW64 is the x86 emulator that allows Win32-based applications to run on 64-bit Windows).
            /// </summary>
            ProcessWow64Information = 0x1A,

            /// <summary>
            /// Retrieves a UNICODE_STRING value containing the name of the image file for the process.
            /// </summary>
            ProcessImageFileName = 0x1B
        }

        /// <summary>
        /// The structure of the Process Environment Block.
        /// </summary>
        /// <remarks>
        /// Tested on Windows 7 x64, 2013-03-10
        /// Source: http://blog.rewolf.pl/blog/?p=573#.UTyBo1fJL6p
        /// </remarks>
        public enum PebStructure32
        {
            InheritedAddressSpace = 0x0,

            ReadImageFileExecOptions = 0x1,

            BeingDebugged = 0x2,

            BitField = 0x3,

            Mutant = 0x4,

            ImageBaseAddress = 0x8,

            Ldr = 0xC,

            ProcessParameters = 0x10,

            SubSystemData = 0x14,

            ProcessHeap = 0x18,

            FastPebLock = 0x1C,

            AtlThunkSListPtr = 0x20,

            IFEOKey = 0x24,

            CrossProcessFlags = 0x28,

            KernelCallbackTable = 0x2C,

            SystemReserved = 0x30,

            AtlThunkSListPtr32 = 0x34,

            ApiSetMap = 0x38,

            TlsExpansionCounter = 0x3C,

            TlsBitmap = 0x40,

            TlsBitmapBits = 0x44,

            ReadOnlySharedMemoryBase = 0x4C,

            SparePvoid0 = 0x50,

            ReadOnlyStaticServerData = 0x54,

            AnsiCodePageData = 0x58,

            OemCodePageData = 0x5C,

            UnicodeCaseTableData = 0x60,

            NumberOfProcessors = 0x64,

            NtGlobalFlag = 0x68,

            CriticalSectionTimeout = 0x70,

            HeapSegmentReserve = 0x78,

            HeapSegmentCommit = 0x7C,

            HeapDeCommitTotalFreeThreshold = 0x80,

            HeapDeCommitFreeBlockThreshold = 0x84,

            NumberOfHeaps = 0x88,

            MaximumNumberOfHeaps = 0x8C,

            ProcessHeaps = 0x90,

            GdiSharedHandleTable = 0x94,

            ProcessStarterHelper = 0x98,

            GdiDcAttributeList = 0x9C,

            LoaderLock = 0xA0,

            OsMajorVersion = 0xA4,

            OsMinorVersion = 0xA8,

            OsBuildNumber = 0xAC,

            OsCsdVersion = 0xAE,

            OsPlatformId = 0xB0,

            ImageSubsystem = 0xB4,

            ImageSubsystemMajorVersion = 0xB8,

            ImageSubsystemMinorVersion = 0xBC,

            ActiveProcessAffinityMask = 0xC0,

            GdiHandleBuffer = 0xC4,

            PostProcessInitRoutine = 0x14C,

            TlsExpansionBitmap = 0x150,

            TlsExpansionBitmapBits = 0x154,

            SessionId = 0x1D4,

            AppCompatFlags = 0x1D8,

            AppCompatFlagsUser = 0x1E0,

            PShimData = 0x1E8,

            AppCompatInfo = 0x1EC,

            CsdVersion = 0x1F0,

            ActivationContextData = 0x1F8,

            ProcessAssemblyStorageMap = 0x1FC,

            SystemDefaultActivationContextData = 0x200,

            SystemAssemblyStorageMap = 0x204,

            MinimumStackCommit = 0x208,

            FlsCallback = 0x20C,

            FlsListHead = 0x210,

            FlsBitmap = 0x218,

            FlsBitmapBits = 0x21C,

            FlsHighIndex = 0x22C,

            WerRegistrationData = 0x230,

            WerShipAssertPtr = 0x234,

            PUnused = 0x238,

            PImageHeaderHash = 0x23C,

            TracingFlags = 0x240,

            CsrServerReadOnlySharedMemoryBase = 0x248,

            TppWorkerpListLock = 0x250,

            TppWorkerpList = 0x254,

            WaitOnAddressHashTable = 0x25C
        }

        /// <summary>
        /// The structure of the Process Environment Block.
        /// </summary>
        /// <remarks>
        /// Tested on Windows 7 x64, 2013-03-10
        /// Source: http://blog.rewolf.pl/blog/?p=573#.UTyBo1fJL6p
        /// </remarks>
        public enum PebStructure64
        {
            InheritedAddressSpace = 0x0,

            ReadImageFileExecOptions = 0x1,

            BeingDebugged = 0x2,

            BitField = 0x3,

            Mutant = 0x8,

            ImageBaseAddress = 0x10,

            Ldr = 0x18,

            ProcessParameters = 0x20,

            SubSystemData = 0x28,

            ProcessHeap = 0x30,

            FastPebLock = 0x38,

            AtlThunkSListPtr = 0x40,

            IFEOKey = 0x48,

            CrossProcessFlags = 0x50,

            Padding1 = 0x54,

            KernelCallbackTable = 0x58,

            SystemReserved = 0x60,

            AtlThunkSListPtr32 = 0x64,

            ApiSetMap = 0x68,

            TlsExpansionCounter = 0x70,

            Padding2 = 0x74,

            TlsBitmap = 0x78,

            TlsBitmapBits = 0x80,

            ReadOnlySharedMemoryBase = 0x88,

            SparePvoid0 = 0x90,

            ReadOnlyStaticServerData = 0x98,

            AnsiCodePageData = 0xA0,

            OemCodePageData = 0xA8,

            UnicodeCaseTableData = 0xB0,

            NumberOfProcessors = 0xB8,

            NtGlobalFlag = 0xBC,

            CriticalSectionTimeout = 0xC0,

            HeapSegmentReserve = 0xC8,

            HeapSegmentCommit = 0xD0,

            HeapDeCommitTotalFreeThreshold = 0xD8,

            HeapDeCommitFreeBlockThreshold = 0xE0,

            NumberOfHeaps = 0xE8,

            MaximumNumberOfHeaps = 0xEC,

            ProcessHeaps = 0xF0,

            GdiSharedHandleTable = 0xF8,

            ProcessStarterHelper = 0x100,

            GdiDcAttributeList = 0x108,

            Padding3 = 0x10C,

            LoaderLock = 0x110,

            OsMajorVersion = 0x118,

            OsMinorVersion = 0x11C,

            OsBuildNumber = 0x120,

            OsCsdVersion = 0x122,

            OsPlatformId = 0x124,

            ImageSubsystem = 0x128,

            ImageSubsystemMajorVersion = 0x12C,

            ImageSubsystemMinorVersion = 0x130,

            Padding4 = 0x134,

            ImageProcessAffinityMask = 0x138,

            GdiHandleBuffer = 0x140,

            PostProcessInitRoutine = 0x230,

            TlsExpansionBitmap = 0x238,

            TlsExpansionBitmapBits = 0x240,

            SessionId = 0x2C0,

            Padding5 = 0x2C4,

            AppCompatFlags = 0x2C8,

            AppCompatFlagsUser = 0x1D0,

            ShimData = 0x2D8,

            AppCompatInfo = 0x2E0,

            CsdVersion = 0x2E8,

            ActivationContextData = 0x2F8,

            ProcessAssemblyStorageMap = 0x300,

            SystemDefaultActivationContextData = 0x308,

            SystemAssemblyStorageMap = 0x310,

            MinimumStackCommit = 0x318,

            FlsCallback = 0x320,

            FlsListHead = 0x328,

            FlsBitmap = 0x338,

            FlsBitmapBits = 0x340,

            FlsHighIndex = 0x350,

            WerRegistrationData = 0x358,

            WerShipAssertPtr = 0x360,

            PUnused = 0x368,

            PImageHeaderHash = 0x370,

            TracingFlags = 0x378,

            Padding6 = 0x37C,

            CsrServerReadOnlySharedMemoryBase = 0x380,

            TppWorkerpListLock = 0x388,

            TppWorkerpList = 0x390,

            WaitOnAddressHashTable = 0x3A0
        }

        /// <summary>
        /// A filtering flag for querying modules in an external process.
        /// </summary>
        internal enum ModuleFilter
        {
            /// <summary>
            /// Use the default behavior.
            /// </summary>
            ListModulesDefault = 0x0,

            /// <summary>
            /// List the 32-bit modules.
            /// </summary>
            ListModules32Bit = 0x01,

            /// <summary>
            /// List the 64-bit modules.
            /// </summary>
            ListModules64Bit = 0x02,

            /// <summary>
            /// List all modules.
            /// </summary>
            ListModulesAll = 0x03,
        }

        /// <summary>
        /// Memory-allocation options list
        /// </summary>
        [Flags]
        internal enum MemoryAllocationFlags
        {
            /// <summary>
            /// Allocates memory charges (from the overall size of memory and the paging files on disk) for the specified reserved memory pages. 
            /// The function also guarantees that when the caller later initially accesses the memory, the contents will be zero. 
            /// Actual physical pages are not allocated unless/until the virtual addresses are actually accessed.
            /// To reserve and commit pages in one step, call <see cref="NativeMethods.VirtualAllocEx"/> with MEM_COMMIT | MEM_RESERVE.
            /// The function fails if you attempt to commit a page that has not been reserved. The resulting error code is ERROR_INVALID_ADDRESS.
            /// An attempt to commit a page that is already committed does not cause the function to fail. 
            /// This means that you can commit pages without first determining the current commitment state of each page
            /// </summary>
            Commit = 0x00001000,

            /// <summary>
            /// Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or in the paging file on disk.
            /// You commit reserved pages by calling <see cref="NativeMethods.VirtualAllocEx"/> again with MEM_COMMIT. 
            /// To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
            /// Other memory allocation functions, such as malloc and LocalAlloc, cannot use reserved memory until it has been released
            /// </summary>
            Reserve = 0x00002000,

            /// <summary>
            /// Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. 
            /// The pages should not be read from or written to the paging file.
            ///  However, the memory block will be used again later, so it should not be decommitted. This value cannot be used with any other value.
            /// Using this value does not guarantee that the range operated on with MEM_RESET will contain zeros. If you want the range to contain zeros, decommit the memory and then recommit it.
            /// When you use MEM_RESET, the VirtualAllocEx function ignores the value of fProtect. However, you must still set fProtect to a valid protection value, such as PAGE_NOACCESS.
            /// <see cref="NativeMethods.VirtualAllocEx"/> returns an error if you use MEM_RESET and the range of memory is mapped to a file. 
            /// A shared view is only acceptable if it is mapped to a paging file
            /// </summary>
            Reset = 0x00080000,

            /// <summary>
            /// MEM_RESET_UNDO should only be called on an address range to which MEM_RESET was successfully applied earlier. 
            /// It indicates that the data in the specified memory range specified by lpAddress and dwSize is of interest to the caller and attempts to reverse the effects of MEM_RESET. 
            /// If the function succeeds, that means all data in the specified address range is intact. 
            /// If the function fails, at least some of the data in the address range has been replaced with zeroes.
            /// This value cannot be used with any other value. 
            /// If MEM_RESET_UNDO is called on an address range which was not MEM_RESET earlier, the behavior is undefined. 
            /// When you specify MEM_RESET, the <see cref="NativeMethods.VirtualAllocEx"/> function ignores the value of flProtect. 
            /// However, you must still set flProtect to a valid protection value, such as PAGE_NOACCESS
            /// </summary>
            ResetUndo = 0x1000000,

            /// <summary>
            /// Allocates memory using large page support.
            /// The size and alignment must be a multiple of the large-page minimum. To obtain this value, use the GetLargePageMinimum function
            /// </summary>
            LargePages = 0x20000000,

            /// <summary>
            /// Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
            /// This value must be used with MEM_RESERVE and no other values
            /// </summary>
            Physical = 0x00400000,

            /// <summary>
            /// Allocates memory at the highest possible address. This can be slower than regular allocations, especially when there are many allocations
            /// </summary>
            TopDown = 0x00100000
        }

        /// <summary>
        /// Memory-protection options list
        /// </summary>
        [Flags]
        internal enum MemoryProtectionFlags
        {
            /// <summary>
            /// Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation.
            /// This value is not officially present in the Microsoft's enumeration but can occur according to the MEMORY_BASIC_INFORMATION structure documentation
            /// </summary>
            ZeroAccess = 0x0,

            /// <summary>
            /// Enables execute access to the committed region of pages. An attempt to read from or write to the committed region results in an access violation.
            /// This flag is not supported by the CreateFileMapping function
            /// </summary>
            Execute = 0x10,

            /// <summary>
            /// Enables execute or read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation
            /// </summary>
            ExecuteRead = 0x20,

            /// <summary>
            /// Enables execute, read-only, or read/write access to the committed region of pages
            /// </summary>
            ExecuteReadWrite = 0x40,

            /// <summary>
            /// Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. 
            /// An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process. 
            /// The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
            /// This flag is not supported by the VirtualAlloc or <see cref="NativeMethods.VirtualAllocEx"/> functions
            /// </summary>
            ExecuteWriteCopy = 0x80,

            /// <summary>
            /// Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation.
            /// This flag is not supported by the CreateFileMapping function
            /// </summary>
            NoAccess = 0x01,

            /// <summary>
            /// Enables read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation. 
            /// If Data Execution Prevention is enabled, an attempt to execute code in the committed region results in an access violation
            /// </summary>
            ReadOnly = 0x02,

            /// <summary>
            /// Enables read-only or read/write access to the committed region of pages. 
            /// If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation
            /// </summary>
            ReadWrite = 0x04,

            /// <summary>
            /// Enables read-only or copy-on-write access to a mapped view of a file mapping object. 
            /// An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process. 
            /// The private page is marked as PAGE_READWRITE, and the change is written to the new page. 
            /// If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
            /// This flag is not supported by the VirtualAlloc or <see cref="NativeMethods.VirtualAllocEx"/> functions
            /// </summary>
            WriteCopy = 0x08,

            /// <summary>
            /// Pages in the region become guard pages. 
            /// Any attempt to access a guard page causes the system to raise a STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. 
            /// Guard pages thus act as a one-time access alarm. For more information, see Creating Guard Pages.
            /// When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
            /// If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
            /// This value cannot be used with PAGE_NOACCESS.
            /// This flag is not supported by the CreateFileMapping function
            /// </summary>
            Guard = 0x100,

            /// <summary>
            /// Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a device. 
            /// Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
            /// The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.
            /// The PAGE_NOCACHE flag can be used only when allocating private memory with the VirtualAlloc, <see cref="NativeMethods.VirtualAllocEx"/>, or VirtualAllocExNuma functions. 
            /// To enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag when calling the CreateFileMapping function
            /// </summary>
            NoCache = 0x200,

            /// <summary>
            /// Sets all pages to be write-combined.
            /// Applications should not use this attribute except when explicitly required for a device. 
            /// Using the interlocked functions with memory that is mapped as write-combined can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
            /// The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags.
            /// The PAGE_WRITECOMBINE flag can be used only when allocating private memory with the VirtualAlloc, <see cref="NativeMethods.VirtualAllocEx"/>, or VirtualAllocExNuma functions. 
            /// To enable write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when calling the CreateFileMapping function
            /// </summary>
            WriteCombine = 0x400
        }

        /// <summary>
        /// Memory-release options list.
        /// </summary>
        [Flags]
        internal enum MemoryReleaseFlags
        {
            /// <summary>
            /// Decommits the specified region of committed pages. After the operation, the pages are in the reserved state.
            /// The function does not fail if you attempt to decommit an uncommitted page. 
            /// This means that you can decommit a range of pages without first determining their current commitment state.
            /// Do not use this value with MEM_RELEASE
            /// </summary>
            Decommit = 0x4000,

            /// <summary>
            /// Releases the specified region of pages. After the operation, the pages are in the free state.
            /// If you specify this value, dwSize must be 0 (zero), and lpAddress must point to the base address returned by the VirtualAllocEx function when the region is reserved. 
            /// The function fails if either of these conditions is not met.
            /// If any pages in the region are committed currently, the function first decommits, and then releases them.
            /// The function does not fail if you attempt to release pages that are in different states, some reserved and some committed. 
            /// This means that you can release a range of pages without first determining the current commitment state.
            /// Do not use this value with MEM_DECOMMIT
            /// </summary>
            Release = 0x8000
        }

        /// <summary>
        /// Memory-state options list.
        /// </summary>
        [Flags]
        internal enum MemoryStateFlags
        {
            /// <summary>
            /// Indicates committed pages for which physical storage has been allocated, either in memory or in the paging file on disk
            /// </summary>
            Commit = 0x1000,

            /// <summary>
            /// Indicates free pages not accessible to the calling process and available to be allocated. 
            /// For free pages, the information in the AllocationBase, AllocationProtect, Protect, and Type members is undefined
            /// </summary>
            Free = 0x10000,

            /// <summary>
            /// Indicates reserved pages where a range of the process's virtual address space is reserved without any physical storage being allocated. 
            /// For reserved pages, the information in the Protect member is undefined
            /// </summary>
            Reserve = 0x2000
        }

        /// <summary>
        /// Memory-type options list.
        /// </summary>
        [Flags]
        internal enum MemoryTypeFlags
        {
            /// <summary>
            /// Non-official flag that is present when the page does not fall into another category
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Indicates that the memory pages within the region are mapped into the view of an image section
            /// </summary>
            Image = 0x1000000,

            /// <summary>
            /// Indicates that the memory pages within the region are mapped into the view of a section
            /// </summary>
            Mapped = 0x40000,

            /// <summary>
            /// Indicates that the memory pages within the region are private (that is, not shared by other processes)
            /// </summary>
            Private = 0x20000
        }
    }
    //// End class
}
//// End namespace