// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

#pragma warning disable 1591

namespace Microsoft.Diagnostics.Runtime.Interop
{
    public enum EXCEPTION : uint
    {
        BCC_FATAL = 0x0EEFFACE,
        BCC_NORMAL = 0x0EEDFAE6,
        DBG_CONTROL_C = 0x4001005,
        DBG_CONTROL_BREAK = 0x4001008,
        DBG_COMMAND_EXCEPTION = 0x4001009,
        WOW_SINGLE_STEP = 0x4000001e,
        WOW_BREAKPOINT = 0x4000001f,
        WINRT_ORIGINATE_ERROR = 0x40080201,
        WINRT_TRANSFORM_ERROR = 0x40080202,
        VISUAL_CPLUSPLUS = 0x406D1388,
        GUARD_PAGE = 0x80000001,
        DATATYPE_MISALIGNMENT = 0x80000002,
        BREAKPOINT = 0x80000003,
        SINGLE_STEP = 0x80000004,
        ACCESS_VIOLATION = 0xC0000005,
        IN_PAGE_ERROR = 0xC0000006,
        WAKE_DEBUGGER = 0x80000007,
        INVALID_HANDLE = 0xC0000008,
        ILLEGAL_INSTRUCTION = 0xC000001D,
        NONCONTINUABLE_EXCEPTION = 0xC0000025,
        INVALID_DISPOSITION = 0xC0000026,
        THREEC_PORT_DISCONNECTED = 0xC0000037,
        ARRAY_BOUNDS_EXCEEDED = 0xC000008C,
        FLT_DENORMAL_OPERAND = 0xC000008D,
        FLT_DIVIDE_BY_ZERO = 0xC000008E,
        FLT_INEXACT_RESULT = 0xC000008F,
        FLT_INVALID_OPERATION = 0xC0000090,
        FLT_OVERFLOW = 0xC0000091,
        FLT_STACK_CHECK = 0xC0000092,
        FLT_UNDERFLOW = 0xC0000093,
        INT_DIVIDE_BY_ZERO = 0xC0000094,
        INT_OVERFLOW = 0xC0000095,
        PRIV_INSTRUCTION = 0xC0000096,
        INVALID_SYSTEM_CALL = 0xC000001C,
        LOCK_SEQUENCE_INVALID = 0xC000001E,
        STACK_OVERFLOW = 0xC00000FD,
        SECURITY_CHECK_FAILURE_STACK_BUFFER_OVERRUN = 0xC0000409,
        ASSERTION_FAILURE = 0xC0000420,
        VERIFIER_STOP = 0xC0000421,
        APPLICATION_HANG = 0xCFFFFFFF,
        MANAGED_NETV4 = 0xE0434352,
        MANAGED_NET = 0xE0434F4D,
        STACK_OVERFLOW_2 = 0xE053534F,
        MSC_CPLUSPLUS = 0xE06D7363,
    }

    public enum SymTag : uint
    {
        Null,                //  0
        Exe,                 //  1
        Compiland,           //  2
        CompilandDetails,    //  3
        CompilandEnv,        //  4
        Function,            //  5
        Block,               //  6
        Data,                //  7
        Annotation,          //  8
        Label,               //  9
        PublicSymbol,        // 10
        UDT,                 // 11
        Enum,                // 12
        FunctionType,        // 13
        PointerType,         // 14
        ArrayType,           // 15
        BaseType,            // 16
        Typedef,             // 17
        BaseClass,           // 18
        Friend,              // 19
        FunctionArgType,     // 20
        FuncDebugStart,      // 21
        FuncDebugEnd,        // 22
        UsingNamespace,      // 23
        VTableShape,         // 24
        VTable,              // 25
        Custom,              // 26
        Thunk,               // 27
        CustomType,          // 28
        ManagedType,         // 29
        Dimension,           // 30
        CallSite,            // 31
        InlineSite,          // 32
        BaseInterface,       // 33
        VectorType,          // 34
        MatrixType,          // 35
        HLSLType,            // 36
        SymTagMax
    }

    public enum DEBUG_REQUEST : uint
    {
        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Unused.
        /// </summary>
        SOURCE_PATH_HAS_SOURCE_SERVER = 0,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Machine-specific CONTEXT.
        /// </summary>
        TARGET_EXCEPTION_CONTEXT = 1,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - ULONG system ID of thread.
        /// </summary>
        TARGET_EXCEPTION_THREAD = 2,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - EXCEPTION_RECORD64.
        /// </summary>
        TARGET_EXCEPTION_RECORD = 3,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - DEBUG_CREATE_PROCESS_OPTIONS.
        /// </summary>
        GET_ADDITIONAL_CREATE_OPTIONS = 4,

        /// <summary>
        /// InBuffer - DEBUG_CREATE_PROCESS_OPTIONS.
        /// OutBuffer - Unused.
        /// </summary>
        SET_ADDITIONAL_CREATE_OPTIONS = 5,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - ULONG[2] major/minor.
        /// </summary>
        GET_WIN32_MAJOR_MINOR_VERSIONS = 6,

        /// <summary>
        /// InBuffer - DEBUG_READ_USER_MINIDUMP_STREAM.
        /// OutBuffer - Unused.
        /// </summary>
        READ_USER_MINIDUMP_STREAM = 7,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Unused.
        /// </summary>
        TARGET_CAN_DETACH = 8,

        /// <summary>
        /// InBuffer - PTSTR.
        /// OutBuffer - Unused.
        /// </summary>
        SET_LOCAL_IMPLICIT_COMMAND_LINE = 9,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Event code stream offset.
        /// </summary>
        GET_CAPTURED_EVENT_CODE_OFFSET = 10,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Event code stream information.
        /// </summary>
        READ_CAPTURED_EVENT_CODE_STREAM = 11,

        /// <summary>
        /// InBuffer - Input data block.
        /// OutBuffer - Processed data block.
        /// </summary>
        EXT_TYPED_DATA_ANSI = 12,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Returned path.
        /// </summary>
        GET_EXTENSION_SEARCH_PATH_WIDE = 13,

        /// <summary>
        /// InBuffer - DEBUG_GET_TEXT_COMPLETIONS_IN.
        /// OutBuffer - DEBUG_GET_TEXT_COMPLETIONS_OUT.
        /// </summary>
        GET_TEXT_COMPLETIONS_WIDE = 14,

        /// <summary>
        /// InBuffer - ULONG64 cookie.
        /// OutBuffer - DEBUG_CACHED_SYMBOL_INFO.
        /// </summary>
        GET_CACHED_SYMBOL_INFO = 15,

        /// <summary>
        /// InBuffer - DEBUG_CACHED_SYMBOL_INFO.
        /// OutBuffer - ULONG64 cookie.
        /// </summary>
        ADD_CACHED_SYMBOL_INFO = 16,

        /// <summary>
        /// InBuffer - ULONG64 cookie.
        /// OutBuffer - Unused.
        /// </summary>
        REMOVE_CACHED_SYMBOL_INFO = 17,

        /// <summary>
        /// InBuffer - DEBUG_GET_TEXT_COMPLETIONS_IN.
        /// OutBuffer - DEBUG_GET_TEXT_COMPLETIONS_OUT.
        /// </summary>
        GET_TEXT_COMPLETIONS_ANSI = 18,

        /// <summary>
        /// InBuffer - Unused.
        /// OutBuffer - Unused.
        /// </summary>
        CURRENT_OUTPUT_CALLBACKS_ARE_DML_AWARE = 19,

        /// <summary>
        /// InBuffer - ULONG64 offset.
        /// OutBuffer - Unwind information.
        /// </summary>
        GET_OFFSET_UNWIND_INFORMATION = 20,

        /// <summary>
        /// InBuffer - Unused
        /// OutBuffer - returned DUMP_HEADER32/DUMP_HEADER64 structure.
        /// </summary>
        GET_DUMP_HEADER = 21,

        /// <summary>
        /// InBuffer - DUMP_HEADER32/DUMP_HEADER64 structure.
        /// OutBuffer - Unused
        /// </summary>
        SET_DUMP_HEADER = 22,

        /// <summary>
        /// InBuffer - Midori specific
        /// OutBuffer - Midori specific
        /// </summary>
        MIDORI = 23,

        /// <summary>
        /// InBuffer - Unused
        /// OutBuffer - PROCESS_NAME_ENTRY blocks
        /// </summary>
        PROCESS_DESCRIPTORS = 24,

        /// <summary>
        /// InBuffer - Unused
        /// OutBuffer - MINIDUMP_MISC_INFO_N blocks
        /// </summary>
        MISC_INFORMATION = 25,

        /// <summary>
        /// InBuffer - Unused
        /// OutBuffer - ULONG64 as TokenHandle value
        /// </summary>
        OPEN_PROCESS_TOKEN = 26,

        /// <summary>
        /// InBuffer - Unused
        /// OutBuffer - ULONG64 as TokenHandle value
        /// </summary>
        OPEN_THREAD_TOKEN = 27,

        /// <summary>
        /// InBuffer -  ULONG64 as TokenHandle being duplicated
        /// OutBuffer - ULONG64 as new duplicated TokenHandle
        /// </summary>
        DUPLICATE_TOKEN = 28,

        /// <summary>
        /// InBuffer - a ULONG64 as TokenHandle and a ULONG as NtQueryInformationToken() request code
        /// OutBuffer - NtQueryInformationToken() return
        /// </summary>
        QUERY_INFO_TOKEN = 29,

        /// <summary>
        /// InBuffer - ULONG64 as TokenHandle
        /// OutBuffer - Unused
        /// </summary>
        CLOSE_TOKEN = 30,

        /// <summary>
        /// InBuffer - ULONG64 for process server identification and ULONG as PID
        /// OutBuffer - Unused
        /// </summary>
        WOW_PROCESS = 31,

        /// <summary>
        /// InBuffer - ULONG64 for process server identification and PWSTR as module path
        /// OutBuffer - Unused
        /// </summary>
        WOW_MODULE = 32,

        /// <summary>
        /// InBuffer - Unused
        /// OutBuffer - Unused
        /// return - S_OK if non-invasive user-mode attach, S_FALSE if not (but still live user-mode), E_FAIL otherwise.
        /// </summary>
        LIVE_USER_NON_INVASIVE = 33,

        /// <summary>
        /// InBuffer - TID
        /// OutBuffer - Unused
        /// return - ResumeThreads() return.
        /// </summary>
        RESUME_THREAD = 34,
    }

    public enum DEBUG_SRCFILE : uint
    {
        SYMBOL_TOKEN = 0,
        SYMBOL_TOKEN_SOURCE_COMMAND_WIDE = 1,
    }

    public enum DEBUG_SYMINFO : uint
    {
        BREAKPOINT_SOURCE_LINE = 0,
        IMAGEHLP_MODULEW64 = 1,
        GET_SYMBOL_NAME_BY_OFFSET_AND_TAG_WIDE = 2,
        GET_MODULE_SYMBOL_NAMES_AND_OFFSETS = 3,
    }

    public enum DEBUG_SYSOBJINFO : uint
    {
        THREAD_BASIC_INFORMATION = 0,
        THREAD_NAME_WIDE = 1,
        CURRENT_PROCESS_COOKIE = 2,
    }

    [Flags]
    public enum DEBUG_TBINFO : uint
    {
        NONE = 0,
        EXIT_STATUS = 1,
        PRIORITY_CLASS = 2,
        PRIORITY = 4,
        TIMES = 8,
        START_OFFSET = 0x10,
        AFFINITY = 0x20,
        ALL = 0x3f
    }

    [Flags]
    public enum DEBUG_GET_TEXT_COMPLETIONS : uint
    {
        NONE = 0,
        NO_DOT_COMMANDS = 1,
        NO_EXTENSION_COMMANDS = 2,
        NO_SYMBOLS = 4,
        IS_DOT_COMMAND = 1,
        IS_EXTENSION_COMMAND = 2,
        IS_SYMBOL = 4,
    }

    public enum DEBUG_CLASS : uint
    {
        UNINITIALIZED = 0,
        KERNEL = 1,
        USER_WINDOWS = 2,
        IMAGE_FILE = 3,
    }

    public enum DEBUG_CLASS_QUALIFIER : uint
    {
        KERNEL_CONNECTION = 0,
        KERNEL_LOCAL = 1,
        KERNEL_EXDI_DRIVER = 2,
        KERNEL_IDNA = 3,
        KERNEL_SMALL_DUMP = 1024,
        KERNEL_DUMP = 1025,
        KERNEL_FULL_DUMP = 1026,
        USER_WINDOWS_PROCESS = 0,
        USER_WINDOWS_PROCESS_SERVER = 1,
        USER_WINDOWS_IDNA = 2,
        USER_WINDOWS_SMALL_DUMP = 1024,
        USER_WINDOWS_DUMP = 1026,
    }

    [Flags]
    public enum DEBUG_ATTACH : uint
    {
        KERNEL_CONNECTION = 0,
        LOCAL_KERNEL = 1,
        EXDI_DRIVER = 2,

        DEFAULT = 0,
        NONINVASIVE = 1,
        EXISTING = 2,
        NONINVASIVE_NO_SUSPEND = 4,
        INVASIVE_NO_INITIAL_BREAK = 8,
        INVASIVE_RESUME_PROCESS = 0x10,
        NONINVASIVE_ALLOW_PARTIAL = 0x20,
    }

    [Flags]
    public enum DEBUG_GET_PROC : uint
    {
        DEFAULT = 0,
        FULL_MATCH = 1,
        ONLY_MATCH = 2,
        SERVICE_NAME = 4,
    }

    [Flags]
    public enum DEBUG_PROC_DESC : uint
    {
        DEFAULT = 0,
        NO_PATHS = 1,
        NO_SERVICES = 2,
        NO_MTS_PACKAGES = 4,
        NO_COMMAND_LINE = 8,
        NO_SESSION_ID = 0x10,
        NO_USER_NAME = 0x20,
    }

    [Flags]
    public enum DEBUG_CREATE_PROCESS : uint
    {
        DEFAULT = 0,
        NO_DEBUG_HEAP = 0x00000400, /* CREATE_UNICODE_ENVIRONMENT */
        THROUGH_RTL = 0x00010000, /* STACK_SIZE_PARAM_IS_A_RESERVATION */
    }

    [Flags]
    public enum DEBUG_ECREATE_PROCESS : uint
    {
        DEFAULT = 0,
        INHERIT_HANDLES = 1,
        USE_VERIFIER_FLAGS = 2,
        USE_IMPLICIT_COMMAND_LINE = 4,
    }

    [Flags]
    public enum DEBUG_PROCESS : uint
    {
        DEFAULT = 0,
        DETACH_ON_EXIT = 1,
        ONLY_THIS_PROCESS = 2,
    }

    public enum DEBUG_DUMP : uint
    {
        SMALL = 1024,
        DEFAULT = 1025,
        FULL = 1026,
        IMAGE_FILE = 1027,
        TRACE_LOG = 1028,
        WINDOWS_CD = 1029,
        KERNEL_DUMP = 1025,
        KERNEL_SMALL_DUMP = 1024,
        KERNEL_FULL_DUMP = 1026,
    }

    [Flags]
    public enum DEBUG_CONNECT_SESSION : uint
    {
        DEFAULT = 0,
        NO_VERSION = 1,
        NO_ANNOUNCE = 2,
    }

    [Flags]
    public enum DEBUG_OUTCTL : uint
    {
        THIS_CLIENT = 0,
        ALL_CLIENTS = 1,
        ALL_OTHER_CLIENTS = 2,
        IGNORE = 3,
        LOG_ONLY = 4,
        SEND_MASK = 7,
        NOT_LOGGED = 8,
        OVERRIDE_MASK = 0x10,
        DML = 0x20,
        AMBIENT_DML = 0xfffffffe,
        AMBIENT_TEXT = 0xffffffff,
    }

    public enum DEBUG_SERVERS : uint
    {
        DEBUGGER = 1,
        PROCESS = 2,
        ALL = 3,
    }

    public enum DEBUG_END : uint
    {
        PASSIVE = 0,
        ACTIVE_TERMINATE = 1,
        ACTIVE_DETACH = 2,
        END_REENTRANT = 3,
        END_DISCONNECT = 4,
    }

    [Flags]
    public enum DEBUG_OUTPUT : uint
    {
        NORMAL = 1,
        ERROR = 2,
        WARNING = 4,
        VERBOSE = 8,
        PROMPT = 0x10,
        PROMPT_REGISTERS = 0x20,
        EXTENSION_WARNING = 0x40,
        DEBUGGEE = 0x80,
        DEBUGGEE_PROMPT = 0x100,
        SYMBOLS = 0x200,
    }

    [Flags]
    public enum DEBUG_EVENT : uint
    {
        NONE = 0,
        BREAKPOINT = 1,
        EXCEPTION = 2,
        CREATE_THREAD = 4,
        EXIT_THREAD = 8,
        CREATE_PROCESS = 0x10,
        EXIT_PROCESS = 0x20,
        LOAD_MODULE = 0x40,
        UNLOAD_MODULE = 0x80,
        SYSTEM_ERROR = 0x100,
        SESSION_STATUS = 0x200,
        CHANGE_DEBUGGEE_STATE = 0x400,
        CHANGE_ENGINE_STATE = 0x800,
        CHANGE_SYMBOL_STATE = 0x1000,
    }

    public enum DEBUG_SESSION : uint
    {
        ACTIVE = 0,
        END_SESSION_ACTIVE_TERMINATE = 1,
        END_SESSION_ACTIVE_DETACH = 2,
        END_SESSION_PASSIVE = 3,
        END = 4,
        REBOOT = 5,
        HIBERNATE = 6,
        FAILURE = 7,
    }

    [Flags]
    public enum DEBUG_CDS : uint
    {
        ALL = 0xffffffff,
        REGISTERS = 1,
        DATA = 2,
        REFRESH = 4, // Inform the GUI clients to refresh debugger windows.
    }

    // What windows should the GUI client refresh?
    [Flags]
    public enum DEBUG_CDS_REFRESH : uint
    {
        EVALUATE = 1,
        EXECUTE = 2,
        EXECUTECOMMANDFILE = 3,
        ADDBREAKPOINT = 4,
        REMOVEBREAKPOINT = 5,
        WRITEVIRTUAL = 6,
        WRITEVIRTUALUNCACHED = 7,
        WRITEPHYSICAL = 8,
        WRITEPHYSICAL2 = 9,
        SETVALUE = 10,
        SETVALUE2 = 11,
        SETSCOPE = 12,
        SETSCOPEFRAMEBYINDEX = 13,
        SETSCOPEFROMJITDEBUGINFO = 14,
        SETSCOPEFROMSTOREDEVENT = 15,
        INLINESTEP = 16,
        INLINESTEP_PSEUDO = 17,
    }

    [Flags]
    public enum DEBUG_CES : uint
    {
        ALL = 0xffffffff,
        CURRENT_THREAD = 1,
        EFFECTIVE_PROCESSOR = 2,
        BREAKPOINTS = 4,
        CODE_LEVEL = 8,
        EXECUTION_STATUS = 0x10,
        ENGINE_OPTIONS = 0x20,
        LOG_FILE = 0x40,
        RADIX = 0x80,
        EVENT_FILTERS = 0x100,
        PROCESS_OPTIONS = 0x200,
        EXTENSIONS = 0x400,
        SYSTEMS = 0x800,
        ASSEMBLY_OPTIONS = 0x1000,
        EXPRESSION_SYNTAX = 0x2000,
        TEXT_REPLACEMENTS = 0x4000,
    }

    [Flags]
    public enum DEBUG_CSS : uint
    {
        ALL = 0xffffffff,
        LOADS = 1,
        UNLOADS = 2,
        SCOPE = 4,
        PATHS = 8,
        SYMBOL_OPTIONS = 0x10,
        TYPE_OPTIONS = 0x20,
    }

    public enum DEBUG_BREAKPOINT_TYPE : uint
    {
        CODE = 0,
        DATA = 1,
        TIME = 2,
    }

    [Flags]
    public enum DEBUG_BREAKPOINT_FLAG : uint
    {
        NONE = 0,
        GO_ONLY = 1,
        DEFERRED = 2,
        ENABLED = 4,
        ADDER_ONLY = 8,
        ONE_SHOT = 0x10,
    }

    [Flags]
    public enum DEBUG_BREAKPOINT_ACCESS_TYPE : uint
    {
        READ = 1,
        WRITE = 2,
        EXECUTE = 4,
        IO = 8,
    }

    [Flags]
    public enum DEBUG_REGISTERS : uint
    {
        DEFAULT = 0,
        INT32 = 1,
        INT64 = 2,
        FLOAT = 4,
        ALL = 7,
    }

    [Flags]
    public enum DEBUG_REGISTER : uint
    {
        SUB_REGISTER = 1,
    }

    public enum DEBUG_VALUE_TYPE : uint
    {
        INVALID = 0,
        INT8 = 1,
        INT16 = 2,
        INT32 = 3,
        INT64 = 4,
        FLOAT32 = 5,
        FLOAT64 = 6,
        FLOAT80 = 7,
        FLOAT82 = 8,
        FLOAT128 = 9,
        VECTOR64 = 10,
        VECTOR128 = 11,
        TYPES = 12,
    }

    public enum INTERFACE_TYPE : int
    {
        InterfaceTypeUndefined = -1,
        Internal,
        Isa,
        Eisa,
        MicroChannel,
        TurboChannel,
        PCIBus,
        VMEBus,
        NuBus,
        PCMCIABus,
        CBus,
        MPIBus,
        MPSABus,
        ProcessorInternal,
        InternalPowerBus,
        PNPISABus,
        PNPBus,
        Vmcs,
        MaximumInterfaceType
    }

    public enum BUS_DATA_TYPE : int
    {
        ConfigurationSpaceUndefined = -1,
        Cmos,
        EisaConfiguration,
        Pos,
        CbusConfiguration,
        PCIConfiguration,
        VMEConfiguration,
        NuBusConfiguration,
        PCMCIAConfiguration,
        MPIConfiguration,
        MPSAConfiguration,
        PNPISAConfiguration,
        SgiInternalConfiguration,
        MaximumBusDataType
    }

    public enum DEBUG_DATA : uint
    {
        KPCR_OFFSET = 0,
        KPRCB_OFFSET = 1,
        KTHREAD_OFFSET = 2,
        BASE_TRANSLATION_VIRTUAL_OFFSET = 3,
        PROCESSOR_IDENTIFICATION = 4,
        PROCESSOR_SPEED = 5,
    }

    [Flags]
    public enum DEBUG_MODULE : uint
    {
        LOADED = 0,
        UNLOADED = 1,
        USER_MODE = 2,
        EXE_MODULE = 4,
        EXPLICIT = 8,
        SECONDARY = 0x10,
        SYNTHETIC = 0x20,
        SYM_BAD_CHECKSUM = 0x10000,
    }

    public enum DEBUG_SYMTYPE : uint
    {
        NONE = 0,
        COFF = 1,
        CODEVIEW = 2,
        PDB = 3,
        EXPORT = 4,
        DEFERRED = 5,
        SYM = 6,
        DIA = 7,
    }

    [Flags]
    public enum DEBUG_OUTTYPE
    {
        DEFAULT = 0,
        NO_INDENT = 1,
        NO_OFFSET = 2,
        VERBOSE = 4,
        COMPACT_OUTPUT = 8,
        ADDRESS_OF_FIELD = 0x10000,
        ADDRESS_ANT_END = 0x20000,
        BLOCK_RECURSE = 0x200000,
    }

    [Flags]
    public enum DEBUG_SCOPE_GROUP : uint
    {
        ARGUMENTS = 1,
        LOCALS = 2,
        ALL = 3,
    }

    [Flags]
    public enum DEBUG_FIND_SOURCE : uint
    {
        DEFAULT = 0,
        FULL_PATH = 1,
        BEST_MATCH = 2,
        NO_SRCSRV = 4,
        TOKEN_LOOKUP = 8,
    }

    [Flags]
    public enum MODULE_ORDERS : uint
    {
        MASK = 0xF0000000,
        LOADTIME = 0x10000000,
        MODULENAME = 0x20000000,
    }

    [Flags]
    public enum SYMOPT : uint
    {
        CASE_INSENSITIVE = 0x00000001,
        UNDNAME = 0x00000002,
        DEFERRED_LOADS = 0x00000004,
        NO_CPP = 0x00000008,
        LOAD_LINES = 0x00000010,
        OMAP_FIND_NEAREST = 0x00000020,
        LOAD_ANYTHING = 0x00000040,
        IGNORE_CVREC = 0x00000080,
        NO_UNQUALIFIED_LOADS = 0x00000100,
        FAIL_CRITICAL_ERRORS = 0x00000200,
        EXACT_SYMBOLS = 0x00000400,
        ALLOW_ABSOLUTE_SYMBOLS = 0x00000800,
        IGNORE_NT_SYMPATH = 0x00001000,
        INCLUDE_32BIT_MODULES = 0x00002000,
        PUBLICS_ONLY = 0x00004000,
        NO_PUBLICS = 0x00008000,
        AUTO_PUBLICS = 0x00010000,
        NO_IMAGE_SEARCH = 0x00020000,
        SECURE = 0x00040000,
        NO_PROMPTS = 0x00080000,
        OVERWRITE = 0x00100000,
        IGNORE_IMAGEDIR = 0x00200000,
        FLAT_DIRECTORY = 0x00400000,
        FAVOR_COMPRESSED = 0x00800000,
        ALLOW_ZERO_ADDRESS = 0x01000000,
        DISABLE_SYMSRV_AUTODETECT = 0x02000000,
        DEBUG = 0x80000000,
    }

    [Flags]
    public enum DEBUG_TYPEOPTS : uint
    {
        UNICODE_DISPLAY = 1,
        LONGSTATUS_DISPLAY = 2,
        FORCERADIX_OUTPUT = 4,
        MATCH_MAXSIZE = 8,
    }

    [Flags]
    public enum DEBUG_SYMBOL : uint
    {
        EXPANSION_LEVEL_MASK = 0xf,
        EXPANDED = 0x10,
        READ_ONLY = 0x20,
        IS_ARRAY = 0x40,
        IS_FLOAT = 0x80,
        IS_ARGUMENT = 0x100,
        IS_LOCAL = 0x200,
    }

    [Flags]
    public enum DEBUG_OUTPUT_SYMBOLS
    {
        DEFAULT = 0,
        NO_NAMES = 1,
        NO_OFFSETS = 2,
        NO_VALUES = 4,
        NO_TYPES = 0x10,
    }

    public enum DEBUG_INTERRUPT : uint
    {
        ACTIVE = 0,
        PASSIVE = 1,
        EXIT = 2,
    }

    [Flags]
    public enum DEBUG_CURRENT : uint
    {
        DEFAULT = 0xf,
        SYMBOL = 1,
        DISASM = 2,
        REGISTERS = 4,
        SOURCE_LINE = 8,
    }

    [Flags]
    public enum DEBUG_DISASM : uint
    {
        EFFECTIVE_ADDRESS = 1,
        MATCHING_SYMBOLS = 2,
        SOURCE_LINE_NUMBER = 4,
        SOURCE_FILE_NAME = 8,
    }

    [Flags]
    public enum DEBUG_STACK : uint
    {
        ARGUMENTS = 0x1,
        FUNCTION_INFO = 0x2,
        SOURCE_LINE = 0x4,
        FRAME_ADDRESSES = 0x8,
        COLUMN_NAMES = 0x10,
        NONVOLATILE_REGISTERS = 0x20,
        FRAME_NUMBERS = 0x40,
        PARAMETERS = 0x80,
        FRAME_ADDRESSES_RA_ONLY = 0x100,
        FRAME_MEMORY_USAGE = 0x200,
        PARAMETERS_NEWLINE = 0x400,
        DML = 0x800,
        FRAME_OFFSETS = 0x1000,
    }

    public enum IMAGE_FILE_MACHINE : uint
    {
        UNKNOWN = 0,
        I386 = 0x014c, // Intel 386.
        R3000 = 0x0162, // MIPS little-endian, 0x160 big-endian
        R4000 = 0x0166, // MIPS little-endian
        R10000 = 0x0168, // MIPS little-endian
        WCEMIPSV2 = 0x0169, // MIPS little-endian WCE v2
        ALPHA = 0x0184, // Alpha_AXP
        SH3 = 0x01a2, // SH3 little-endian
        SH3DSP = 0x01a3,
        SH3E = 0x01a4, // SH3E little-endian
        SH4 = 0x01a6, // SH4 little-endian
        SH5 = 0x01a8, // SH5
        ARM = 0x01c0, // ARM Little-Endian
        THUMB = 0x01c2,
        THUMB2 = 0x1c4,
        AM33 = 0x01d3,
        POWERPC = 0x01F0, // IBM PowerPC Little-Endian
        POWERPCFP = 0x01f1,
        IA64 = 0x0200, // Intel 64
        MIPS16 = 0x0266, // MIPS
        ALPHA64 = 0x0284, // ALPHA64
        MIPSFPU = 0x0366, // MIPS
        MIPSFPU16 = 0x0466, // MIPS
        AXP64 = 0x0284,
        TRICORE = 0x0520, // Infineon
        CEF = 0x0CEF,
        EBC = 0x0EBC, // EFI Byte Code
        AMD64 = 0x8664, // AMD64 (K8)
        M32R = 0x9041, // M32R little-endian
        CEE = 0xC0EE,
    }

    public enum DEBUG_STATUS : uint
    {
        NO_CHANGE = 0,
        GO = 1,
        GO_HANDLED = 2,
        GO_NOT_HANDLED = 3,
        STEP_OVER = 4,
        STEP_INTO = 5,
        BREAK = 6,
        NO_DEBUGGEE = 7,
        STEP_BRANCH = 8,
        IGNORE_EVENT = 9,
        RESTART_REQUESTED = 10,
        REVERSE_GO = 11,
        REVERSE_STEP_BRANCH = 12,
        REVERSE_STEP_OVER = 13,
        REVERSE_STEP_INTO = 14,
        OUT_OF_SYNC = 15,
        WAIT_INPUT = 16,
        TIMEOUT = 17,
        MASK = 0x1f,
    }

    public enum DEBUG_STATUS_FLAGS : ulong
    {
        /// <summary>
        ///    This bit is added in DEBUG_CES_EXECUTION_STATUS notifications when the
        ///    engines execution status is changing due to operations performed during a
        ///    wait, such as making synchronous callbacks. If the bit is not set the
        ///    execution status is changing due to a wait being satisfied.
        /// </summary>
        INSIDE_WAIT = 0x100000000,

        /// <summary>
        ///    This bit is added in DEBUG_CES_EXECUTION_STATUS notifications when the
        ///    engines execution status update is coming after a wait has timed-out. It
        ///    indicates that the execution status change was not due to an actual event.
        /// </summary>
        WAIT_TIMEOUT = 0x200000000
    }

    [Flags]
    public enum DEBUG_CES_EXECUTION_STATUS : ulong
    {
        INSIDE_WAIT = 0x100000000UL,
        WAIT_TIMEOUT = 0x200000000UL,
    }

    public enum DEBUG_LEVEL : uint
    {
        SOURCE = 0,
        ASSEMBLY = 1,
    }

    [Flags]
    public enum DEBUG_ENGOPT : uint
    {
        NONE = 0,
        IGNORE_DBGHELP_VERSION = 0x00000001,
        IGNORE_EXTENSION_VERSIONS = 0x00000002,
        ALLOW_NETWORK_PATHS = 0x00000004,
        DISALLOW_NETWORK_PATHS = 0x00000008,
        NETWORK_PATHS = (0x00000004 | 0x00000008),
        IGNORE_LOADER_EXCEPTIONS = 0x00000010,
        INITIAL_BREAK = 0x00000020,
        INITIAL_MODULE_BREAK = 0x00000040,
        FINAL_BREAK = 0x00000080,
        NO_EXECUTE_REPEAT = 0x00000100,
        FAIL_INCOMPLETE_INFORMATION = 0x00000200,
        ALLOW_READ_ONLY_BREAKPOINTS = 0x00000400,
        SYNCHRONIZE_BREAKPOINTS = 0x00000800,
        DISALLOW_SHELL_COMMANDS = 0x00001000,
        KD_QUIET_MODE = 0x00002000,
        DISABLE_MANAGED_SUPPORT = 0x00004000,
        DISABLE_MODULE_SYMBOL_LOAD = 0x00008000,
        DISABLE_EXECUTION_COMMANDS = 0x00010000,
        DISALLOW_IMAGE_FILE_MAPPING = 0x00020000,
        PREFER_DML = 0x00040000,
        ALL = 0x0007FFFF,
    }

    public enum ERROR_LEVEL
    {
        ERROR = 1,
        MINORERROR = 2,
        WARNING = 3,
    }

    [Flags]
    public enum DEBUG_EXECUTE : uint
    {
        DEFAULT = 0,
        ECHO = 1,
        NOT_LOGGED = 2,
        NO_REPEAT = 4,
    }

    public enum DEBUG_FILTER_EVENT : uint
    {
        CREATE_THREAD = 0x00000000,
        EXIT_THREAD = 0x00000001,
        CREATE_PROCESS = 0x00000002,
        EXIT_PROCESS = 0x00000003,
        LOAD_MODULE = 0x00000004,
        UNLOAD_MODULE = 0x00000005,
        SYSTEM_ERROR = 0x00000006,
        INITIAL_BREAKPOINT = 0x00000007,
        INITIAL_MODULE_LOAD = 0x00000008,
        DEBUGGEE_OUTPUT = 0x00000009,
    }

    public enum DEBUG_FILTER_EXEC_OPTION : uint
    {
        BREAK = 0x00000000,
        SECOND_CHANCE_BREAK = 0x00000001,
        OUTPUT = 0x00000002,
        IGNORE = 0x00000003,
        REMOVE = 0x00000004,
    }

    public enum DEBUG_FILTER_CONTINUE_OPTION : uint
    {
        GO_HANDLED = 0x00000000,
        GO_NOT_HANDLED = 0x00000001,
    }

    [Flags]
    public enum DEBUG_WAIT : uint
    {
        DEFAULT = 0,
    }

    public enum DEBUG_HANDLE_DATA_TYPE : uint
    {
        BASIC = 0,
        TYPE_NAME = 1,
        OBJECT_NAME = 2,
        HANDLE_COUNT = 3,
        TYPE_NAME_WIDE = 4,
        OBJECT_NAME_WIDE = 5,
        MINI_THREAD_1 = 6,
        MINI_MUTANT_1 = 7,
        MINI_MUTANT_2 = 8,
        PER_HANDLE_OPERATIONS = 9,
        ALL_HANDLE_OPERATIONS = 10,
        MINI_PROCESS_1 = 11,
        MINI_PROCESS_2 = 12,
    }

    public enum DEBUG_DATA_SPACE : uint
    {
        VIRTUAL = 0,
        PHYSICAL = 1,
        CONTROL = 2,
        IO = 3,
        MSR = 4,
        BUS_DATA = 5,
        DEBUGGER_DATA = 6,
    }

    public enum DEBUG_OFFSINFO : uint
    {
        VIRTUAL_SOURCE = 0x00000001,
    }

    public enum DEBUG_VSOURCE : uint
    {
        INVALID = 0,
        DEBUGGEE = 1,
        MAPPED_IMAGE = 2,
        DUMP_WITHOUT_MEMINFO = 3,
    }

    public enum DEBUG_VSEARCH : uint
    {
        DEFAULT = 0,
        WRITABLE_ONLY = 1,
    }

    public enum CODE_PAGE : uint
    {
        ACP = 0, // default to ANSI code page
        OEMCP = 1, // default to OEM  code page
        MACCP = 2, // default to MAC  code page
        THREAD_ACP = 3, // current thread's ANSI code page
        SYMBOL = 42, // SYMBOL translations

        UTF7 = 65000, // UTF-7 translation
        UTF8 = 65001, // UTF-8 translation
    }

    public enum DEBUG_PHYSICAL : uint
    {
        DEFAULT = 0,
        CACHED = 1,
        UNCACHED = 2,
        WRITE_COMBINED = 3,
    }

    [Flags]
    public enum DEBUG_OUTCBI : uint
    {
        EXPLICIT_FLUSH = 1,
        TEXT = 2,
        DML = 4,
        ANY_FORMAT = 6,
    }

    public enum DEBUG_OUTCB : uint
    {
        TEXT = 0,
        DML = 1,
        EXPLICIT_FLUSH = 2,
    }

    [Flags]
    public enum DEBUG_OUTCBF : uint
    {
        EXPLICIT_FLUSH = 1,
        DML_HAS_TAGS = 2,
        DML_HAS_SPECIAL_CHARACTERS = 4,
    }

    [Flags]
    public enum DEBUG_FORMAT : uint
    {
        DEFAULT = 0x00000000,
        CAB_SECONDARY_ALL_IMAGES = 0x10000000,
        WRITE_CAB = 0x20000000,
        CAB_SECONDARY_FILES = 0x40000000,
        NO_OVERWRITE = 0x80000000,

        USER_SMALL_FULL_MEMORY = 0x00000001,
        USER_SMALL_HANDLE_DATA = 0x00000002,
        USER_SMALL_UNLOADED_MODULES = 0x00000004,
        USER_SMALL_INDIRECT_MEMORY = 0x00000008,
        USER_SMALL_DATA_SEGMENTS = 0x00000010,
        USER_SMALL_FILTER_MEMORY = 0x00000020,
        USER_SMALL_FILTER_PATHS = 0x00000040,
        USER_SMALL_PROCESS_THREAD_DATA = 0x00000080,
        USER_SMALL_PRIVATE_READ_WRITE_MEMORY = 0x00000100,
        USER_SMALL_NO_OPTIONAL_DATA = 0x00000200,
        USER_SMALL_FULL_MEMORY_INFO = 0x00000400,
        USER_SMALL_THREAD_INFO = 0x00000800,
        USER_SMALL_CODE_SEGMENTS = 0x00001000,
        USER_SMALL_NO_AUXILIARY_STATE = 0x00002000,
        USER_SMALL_FULL_AUXILIARY_STATE = 0x00004000,
        USER_SMALL_IGNORE_INACCESSIBLE_MEM = 0x08000000,
    }

    public enum DEBUG_DUMP_FILE : uint
    {
        BASE = 0xffffffff,
        PAGE_FILE_DUMP = 0,
    }

    [Flags]
    public enum MEM : uint
    {
        COMMIT = 0x1000,
        RESERVE = 0x2000,
        DECOMMIT = 0x4000,
        RELEASE = 0x8000,
        FREE = 0x10000,
        PRIVATE = 0x20000,
        MAPPED = 0x40000,
        RESET = 0x80000,
        TOP_DOWN = 0x100000,
        WRITE_WATCH = 0x200000,
        PHYSICAL = 0x400000,
        ROTATE = 0x800000,
        LARGE_PAGES = 0x20000000,
        FOURMB_PAGES = 0x80000000,

        IMAGE = SEC.IMAGE,
    }

    [Flags]
    public enum PAGE : uint
    {
        NOACCESS = 0x01,
        READONLY = 0x02,
        READWRITE = 0x04,
        WRITECOPY = 0x08,
        EXECUTE = 0x10,
        EXECUTE_READ = 0x20,
        EXECUTE_READWRITE = 0x40,
        EXECUTE_WRITECOPY = 0x80,
        GUARD = 0x100,
        NOCACHE = 0x200,
        WRITECOMBINE = 0x400,
    }

    [Flags]
    public enum SEC : uint
    {
        FILE = 0x800000,
        IMAGE = 0x1000000,
        PROTECTED_IMAGE = 0x2000000,
        RESERVE = 0x4000000,
        COMMIT = 0x8000000,
        NOCACHE = 0x10000000,
        WRITECOMBINE = 0x40000000,
        LARGE_PAGES = 0x80000000,
        MEM_IMAGE = IMAGE,
    }

    public enum DEBUG_MODNAME : uint
    {
        IMAGE = 0x00000000,
        MODULE = 0x00000001,
        LOADED_IMAGE = 0x00000002,
        SYMBOL_FILE = 0x00000003,
        MAPPED_IMAGE = 0x00000004,
    }

    [Flags]
    public enum DEBUG_OUT_TEXT_REPL : uint
    {
        DEFAULT = 0,
    }

    [Flags]
    public enum DEBUG_ASMOPT : uint
    {
        DEFAULT = 0x00000000,
        VERBOSE = 0x00000001,
        NO_CODE_BYTES = 0x00000002,
        IGNORE_OUTPUT_WIDTH = 0x00000004,
        SOURCE_LINE_NUMBER = 0x00000008,
    }

    public enum DEBUG_EXPR : uint
    {
        MASM = 0,
        CPLUSPLUS = 1,
    }

    public enum DEBUG_EINDEX : uint
    {
        NAME = 0,
        FROM_START = 0,
        FROM_END = 1,
        FROM_CURRENT = 2,
    }

    [Flags]
    public enum DEBUG_LOG : uint
    {
        DEFAULT = 0,
        APPEND = 1,
        UNICODE = 2,
        DML = 4,
    }

    public enum DEBUG_SYSVERSTR : uint
    {
        SERVICE_PACK = 0,
        BUILD = 1,
    }

    [Flags]
    public enum DEBUG_MANAGED : uint
    {
        DISABLED = 0,
        ALLOWED = 1,
        DLL_LOADED = 2,
    }

    [Flags]
    public enum DEBUG_MANSTR : uint
    {
        NONE = 0,
        LOADED_SUPPORT_DLL = 1,
        LOAD_STATUS = 2,
    }

    [Flags]
    public enum DEBUG_MANRESET : uint
    {
        DEFAULT = 0,
        LOAD_DLL = 1,
    }

    [Flags]
    public enum VS_FF : uint
    {
        DEBUG = 0x00000001,
        PRERELEASE = 0x00000002,
        PATCHED = 0x00000004,
        PRIVATEBUILD = 0x00000008,
        INFOINFERRED = 0x00000010,
        SPECIALBUILD = 0x00000020,
    }

    public enum MODULE_ARCHITECTURE
    {
        UNKNOWN,
        I386,
        X64,
        IA64,
        ANY,
    }

    public enum IG : ushort
    {
        KD_CONTEXT = 1,
        READ_CONTROL_SPACE = 2,
        WRITE_CONTROL_SPACE = 3,
        READ_IO_SPACE = 4,
        WRITE_IO_SPACE = 5,
        READ_PHYSICAL = 6,
        WRITE_PHYSICAL = 7,
        READ_IO_SPACE_EX = 8,
        WRITE_IO_SPACE_EX = 9,
        KSTACK_HELP = 10, // obsolete
        SET_THREAD = 11,
        READ_MSR = 12,
        WRITE_MSR = 13,
        GET_DEBUGGER_DATA = 14,
        GET_KERNEL_VERSION = 15,
        RELOAD_SYMBOLS = 16,
        GET_SET_SYMPATH = 17,
        GET_EXCEPTION_RECORD = 18,
        IS_PTR64 = 19,
        GET_BUS_DATA = 20,
        SET_BUS_DATA = 21,
        DUMP_SYMBOL_INFO = 22,
        LOWMEM_CHECK = 23,
        SEARCH_MEMORY = 24,
        GET_CURRENT_THREAD = 25,
        GET_CURRENT_PROCESS = 26,
        GET_TYPE_SIZE = 27,
        GET_CURRENT_PROCESS_HANDLE = 28,
        GET_INPUT_LINE = 29,
        GET_EXPRESSION_EX = 30,
        TRANSLATE_VIRTUAL_TO_PHYSICAL = 31,
        GET_CACHE_SIZE = 32,
        READ_PHYSICAL_WITH_FLAGS = 33,
        WRITE_PHYSICAL_WITH_FLAGS = 34,
        POINTER_SEARCH_PHYSICAL = 35,
        OBSOLETE_PLACEHOLDER_36 = 36,
        GET_THREAD_OS_INFO = 37,
        GET_CLR_DATA_INTERFACE = 38,
        MATCH_PATTERN_A = 39,
        FIND_FILE = 40,
        TYPED_DATA_OBSOLETE = 41,
        QUERY_TARGET_INTERFACE = 42,
        TYPED_DATA = 43,
        DISASSEMBLE_BUFFER = 44,
        GET_ANY_MODULE_IN_RANGE = 45,
        VIRTUAL_TO_PHYSICAL = 46,
        PHYSICAL_TO_VIRTUAL = 47,
        GET_CONTEXT_EX = 48,
        GET_TEB_ADDRESS = 128,
        GET_PEB_ADDRESS = 129,
    }

    [Flags]
    public enum EFileAccess : uint
    {
        None = 0x00000000,
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000
    }

    [Flags]
    public enum EFileShare : uint
    {
        None = 0x00000000,
        Read = 0x00000001,
        Write = 0x00000002,
        Delete = 0x00000004
    }

    public enum ECreationDisposition : uint
    {
        /// <summary>
        /// Creates a new file. The function fails if a specified file exists.
        /// </summary>
        New = 1,

        /// <summary>
        /// Creates a new file, always.
        /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
        /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
        /// </summary>
        CreateAlways = 2,

        /// <summary>
        /// Opens a file. The function fails if the file does not exist.
        /// </summary>
        OpenExisting = 3,

        /// <summary>
        /// Opens a file, always.
        /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
        /// </summary>
        OpenAlways = 4,

        /// <summary>
        /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
        /// The calling process must open the file with the GENERIC_WRITE access right.
        /// </summary>
        TruncateExisting = 5
    }

    [Flags]
    public enum EFileAttributes : uint
    {
        Readonly = 0x00000001,
        Hidden = 0x00000002,
        System = 0x00000004,
        Directory = 0x00000010,
        Archive = 0x00000020,
        Device = 0x00000040,
        Normal = 0x00000080,
        Temporary = 0x00000100,
        SparseFile = 0x00000200,
        ReparsePoint = 0x00000400,
        Compressed = 0x00000800,
        Offline = 0x00001000,
        NotContentIndexed = 0x00002000,
        Encrypted = 0x00004000,
        Write_Through = 0x80000000,
        Overlapped = 0x40000000,
        NoBuffering = 0x20000000,
        RandomAccess = 0x10000000,
        SequentialScan = 0x08000000,
        DeleteOnClose = 0x04000000,
        BackupSemantics = 0x02000000,
        PosixSemantics = 0x01000000,
        OpenReparsePoint = 0x00200000,
        OpenNoRecall = 0x00100000,
        FirstPipeInstance = 0x00080000
    }

    public enum SPF_MOVE_METHOD : uint
    {
        FILE_BEGIN = 0,
        FILE_CURRENT = 1,
        FILE_END = 2,
    }

    [Flags]
    public enum DEBUG_GETMOD : uint
    {
        DEFAULT = 0,
        NO_LOADED_MODULES = 1,
        NO_UNLOADED_MODULES = 2,
    }

    [Flags]
    public enum DEBUG_ADDSYNTHMOD : uint
    {
        DEFAULT = 0,
    }

    [Flags]
    public enum DEBUG_ADDSYNTHSYM : uint
    {
        DEFAULT = 0,
    }

    [Flags]
    public enum DEBUG_OUTSYM : uint
    {
        DEFAULT = 0,
        FORCE_OFFSET = 1,
        SOURCE_LINE = 2,
        ALLOW_DISPLACEMENT = 4,
    }

    [Flags]
    public enum DEBUG_GETFNENT : uint
    {
        DEFAULT = 0,
        RAW_ENTRY_ONLY = 1,
    }

    [Flags]
    public enum DEBUG_SOURCE : uint
    {
        IS_STATEMENT = 1,
    }

    [Flags]
    public enum DEBUG_GSEL : uint
    {
        DEFAULT = 0,
        NO_SYMBOL_LOADS = 1,
        ALLOW_LOWER = 2,
        ALLOW_HIGHER = 4,
        NEAREST_ONLY = 8,
        INLINE_CALLSITE = 0x10,
    }

    [Flags]
    public enum DEBUG_FRAME : uint
    {
        DEFAULT = 0,
        IGNORE_INLINE = 1,
    }

    public enum DEBUG_REGSRC : uint
    {
        DEBUGGEE = 0,
        EXPLICIT = 1,
        FRAME = 2,
    }

    public enum _EXT_TDOP
    {
        EXT_TDOP_COPY,
        EXT_TDOP_RELEASE,
        EXT_TDOP_SET_FROM_EXPR,
        EXT_TDOP_SET_FROM_U64_EXPR,
        EXT_TDOP_GET_FIELD,
        EXT_TDOP_EVALUATE,
        EXT_TDOP_GET_TYPE_NAME,
        EXT_TDOP_OUTPUT_TYPE_NAME,
        EXT_TDOP_OUTPUT_SIMPLE_VALUE,
        EXT_TDOP_OUTPUT_FULL_VALUE,
        EXT_TDOP_HAS_FIELD,
        EXT_TDOP_GET_FIELD_OFFSET,
        EXT_TDOP_GET_ARRAY_ELEMENT,
        EXT_TDOP_GET_DEREFERENCE,
        EXT_TDOP_GET_TYPE_SIZE,
        EXT_TDOP_OUTPUT_TYPE_DEFINITION,
        EXT_TDOP_GET_POINTER_TO,
        EXT_TDOP_SET_FROM_TYPE_ID_AND_U64,
        EXT_TDOP_SET_PTR_FROM_TYPE_ID_AND_U64,
        EXT_TDOP_COUNT
    }

    [Flags]
    public enum FORMAT_MESSAGE
    {
        ALLOCATE_BUFFER = 0x0100,
        IGNORE_INSERTS = 0x0200,
        FROM_STRING = 0x0400,
        FROM_HMODULE = 0x0800,
        FROM_SYSTEM = 0x1000,
        ARGUMENT_ARRAY = 0x2000,
    }

    public enum ERROR_CODE
    {
        ERROR_SUCCESS = 0,
        ERROR_INVALID_FUNCTION = 1,
        ERROR_FILE_NOT_FOUND = 2,
        ERROR_PATH_NOT_FOUND = 3,
        ERROR_TOO_MANY_OPEN_FILES = 4,
        ERROR_ACCESS_DENIED = 5,
        ERROR_INVALID_HANDLE = 6,
        ERROR_ARENA_TRASHED = 7,
        ERROR_NOT_ENOUGH_MEMORY = 8,
        ERROR_INVALID_BLOCK = 9,
        ERROR_BAD_ENVIRONMENT = 10,
        ERROR_BAD_FORMAT = 11,
        ERROR_INVALID_ACCESS = 12,
        ERROR_INVALID_DATA = 13,
        ERROR_OUTOFMEMORY = 14,
        ERROR_INVALID_DRIVE = 15,
        ERROR_CURRENT_DIRECTORY = 16,
        ERROR_NOT_SAME_DEVICE = 17,
        ERROR_NO_MORE_FILES = 18,
        ERROR_WRITE_PROTECT = 19,
        ERROR_BAD_UNIT = 20,
        ERROR_NOT_READY = 21,
        ERROR_BAD_COMMAND = 22,
        ERROR_CRC = 23,
        ERROR_BAD_LENGTH = 24,
        ERROR_SEEK = 25,
        ERROR_NOT_DOS_DISK = 26,
        ERROR_SECTOR_NOT_FOUND = 27,
        ERROR_OUT_OF_PAPER = 28,
        ERROR_WRITE_FAULT = 29,
        ERROR_READ_FAULT = 30,
        ERROR_GEN_FAILURE = 31,
        ERROR_SHARING_VIOLATION = 32,
        ERROR_LOCK_VIOLATION = 33,
        ERROR_WRONG_DISK = 34,
        ERROR_SHARING_BUFFER_EXCEEDED = 36,
        ERROR_HANDLE_EOF = 38,
        ERROR_HANDLE_DISK_FULL = 39,
        ERROR_NOT_SUPPORTED = 50,
        ERROR_REM_NOT_LIST = 51,
        ERROR_DUP_NAME = 52,
        ERROR_BAD_NETPATH = 53,
        ERROR_NETWORK_BUSY = 54,
        ERROR_DEV_NOT_EXIST = 55,
        ERROR_TOO_MANY_CMDS = 56,
        ERROR_ADAP_HDW_ERR = 57,
        ERROR_BAD_NET_RESP = 58,
        ERROR_UNEXP_NET_ERR = 59,
        ERROR_BAD_REM_ADAP = 60,
        ERROR_PRINTQ_FULL = 61,
        ERROR_NO_SPOOL_SPACE = 62,
        ERROR_PRINT_CANCELLED = 63,
        ERROR_NETNAME_DELETED = 64,
        ERROR_NETWORK_ACCESS_DENIED = 65,
        ERROR_BAD_DEV_TYPE = 66,
        ERROR_BAD_NET_NAME = 67,
        ERROR_TOO_MANY_NAMES = 68,
        ERROR_TOO_MANY_SESS = 69,
        ERROR_SHARING_PAUSED = 70,
        ERROR_REQ_NOT_ACCEP = 71,
        ERROR_REDIR_PAUSED = 72,
        ERROR_FILE_EXISTS = 80,
        ERROR_CANNOT_MAKE = 82,
        ERROR_FAIL_I24 = 83,
        ERROR_OUT_OF_STRUCTURES = 84,
        ERROR_ALREADY_ASSIGNED = 85,
        ERROR_INVALID_PASSWORD = 86,
        ERROR_INVALID_PARAMETER = 87,
        ERROR_NET_WRITE_FAULT = 88,
        ERROR_NO_PROC_SLOTS = 89,
        ERROR_TOO_MANY_SEMAPHORES = 100,
        ERROR_EXCL_SEM_ALREADY_OWNED = 101,
        ERROR_SEM_IS_SET = 102,
        ERROR_TOO_MANY_SEM_REQUESTS = 103,
        ERROR_INVALID_AT_INTERRUPT_TIME = 104,
        ERROR_SEM_OWNER_DIED = 105,
        ERROR_SEM_USER_LIMIT = 106,
        ERROR_DISK_CHANGE = 107,
        ERROR_DRIVE_LOCKED = 108,
        ERROR_BROKEN_PIPE = 109,
        ERROR_OPEN_FAILED = 110,
        ERROR_BUFFER_OVERFLOW = 111,
        ERROR_DISK_FULL = 112,
        ERROR_NO_MORE_SEARCH_HANDLES = 113,
        ERROR_INVALID_TARGET_HANDLE = 114,
        ERROR_INVALID_CATEGORY = 117,
        ERROR_INVALID_VERIFY_SWITCH = 118,
        ERROR_BAD_DRIVER_LEVEL = 119,
        ERROR_CALL_NOT_IMPLEMENTED = 120,
        ERROR_SEM_TIMEOUT = 121,
        ERROR_INSUFFICIENT_BUFFER = 122,
        ERROR_INVALID_NAME = 123,
        ERROR_INVALID_LEVEL = 124,
        ERROR_NO_VOLUME_LABEL = 125,
        ERROR_MOD_NOT_FOUND = 126,
        ERROR_PROC_NOT_FOUND = 127,
        ERROR_WAIT_NO_CHILDREN = 128,
        ERROR_CHILD_NOT_COMPLETE = 129,
        ERROR_DIRECT_ACCESS_HANDLE = 130,
        ERROR_NEGATIVE_SEEK = 131,
        ERROR_SEEK_ON_DEVICE = 132,
        ERROR_IS_JOIN_TARGET = 133,
        ERROR_IS_JOINED = 134,
        ERROR_IS_SUBSTED = 135,
        ERROR_NOT_JOINED = 136,
        ERROR_NOT_SUBSTED = 137,
        ERROR_JOIN_TO_JOIN = 138,
        ERROR_SUBST_TO_SUBST = 139,
        ERROR_JOIN_TO_SUBST = 140,
        ERROR_SUBST_TO_JOIN = 141,
        ERROR_BUSY_DRIVE = 142,
        ERROR_SAME_DRIVE = 143,
        ERROR_DIR_NOT_ROOT = 144,
        ERROR_DIR_NOT_EMPTY = 145,
        ERROR_IS_SUBST_PATH = 146,
        ERROR_IS_JOIN_PATH = 147,
        ERROR_PATH_BUSY = 148,
        ERROR_IS_SUBST_TARGET = 149,
        ERROR_SYSTEM_TRACE = 150,
        ERROR_INVALID_EVENT_COUNT = 151,
        ERROR_TOO_MANY_MUXWAITERS = 152,
        ERROR_INVALID_LIST_FORMAT = 153,
        ERROR_LABEL_TOO_LONG = 154,
        ERROR_TOO_MANY_TCBS = 155,
        ERROR_SIGNAL_REFUSED = 156,
        ERROR_DISCARDED = 157,
        ERROR_NOT_LOCKED = 158,
        ERROR_BAD_THREADID_ADDR = 159,
        ERROR_BAD_ARGUMENTS = 160,
        ERROR_BAD_PATHNAME = 161,
        ERROR_SIGNAL_PENDING = 162,
        ERROR_MAX_THRDS_REACHED = 164,
        ERROR_LOCK_FAILED = 167,
        ERROR_BUSY = 170,
        ERROR_DEVICE_SUPPORT_IN_PROGRESS = 171,
        ERROR_CANCEL_VIOLATION = 173,
        ERROR_ATOMIC_LOCKS_NOT_SUPPORTED = 174,
        ERROR_INVALID_SEGMENT_NUMBER = 180,
        ERROR_INVALID_ORDINAL = 182,
        ERROR_ALREADY_EXISTS = 183,
        ERROR_INVALID_FLAG_NUMBER = 186,
        ERROR_SEM_NOT_FOUND = 187,
        ERROR_INVALID_STARTING_CODESEG = 188,
        ERROR_INVALID_STACKSEG = 189,
        ERROR_INVALID_MODULETYPE = 190,
        ERROR_INVALID_EXE_SIGNATURE = 191,
        ERROR_EXE_MARKED_INVALID = 192,
        ERROR_BAD_EXE_FORMAT = 193,
        ERROR_ITERATED_DATA_EXCEEDS_64k = 194,
        ERROR_INVALID_MINALLOCSIZE = 195,
        ERROR_DYNLINK_FROM_INVALID_RING = 196,
        ERROR_IOPL_NOT_ENABLED = 197,
        ERROR_INVALID_SEGDPL = 198,
        ERROR_AUTODATASEG_EXCEEDS_64k = 199,
        ERROR_RING2SEG_MUST_BE_MOVABLE = 200,
        ERROR_RELOC_CHAIN_XEEDS_SEGLIM = 201,
        ERROR_INFLOOP_IN_RELOC_CHAIN = 202,
        ERROR_ENVVAR_NOT_FOUND = 203,
        ERROR_NO_SIGNAL_SENT = 205,
        ERROR_FILENAME_EXCED_RANGE = 206,
        ERROR_RING2_STACK_IN_USE = 207,
        ERROR_META_EXPANSION_TOO_LONG = 208,
        ERROR_INVALID_SIGNAL_NUMBER = 209,
        ERROR_THREAD_1_INACTIVE = 210,
        ERROR_LOCKED = 212,
        ERROR_TOO_MANY_MODULES = 214,
        ERROR_NESTING_NOT_ALLOWED = 215,
        ERROR_EXE_MACHINE_TYPE_MISMATCH = 216,
        ERROR_EXE_CANNOT_MODIFY_SIGNED_BINARY = 217,
        ERROR_EXE_CANNOT_MODIFY_STRONG_SIGNED_BINARY = 218,
        ERROR_FILE_CHECKED_OUT = 220,
        ERROR_CHECKOUT_REQUIRED = 221,
        ERROR_BAD_FILE_TYPE = 222,
        ERROR_FILE_TOO_LARGE = 223,
        ERROR_FORMS_AUTH_REQUIRED = 224,
        ERROR_VIRUS_INFECTED = 225,
        ERROR_VIRUS_DELETED = 226,
        ERROR_PIPE_LOCAL = 229,
        ERROR_BAD_PIPE = 230,
        ERROR_PIPE_BUSY = 231,
        ERROR_NO_DATA = 232,
        ERROR_PIPE_NOT_CONNECTED = 233,
        ERROR_MORE_DATA = 234,
        ERROR_VC_DISCONNECTED = 240,
        ERROR_INVALID_EA_NAME = 254,
        ERROR_EA_LIST_INCONSISTENT = 255,
        ERROR_NO_MORE_ITEMS = 259,
        ERROR_CANNOT_COPY = 266,
        ERROR_DIRECTORY = 267,
        ERROR_EAS_DIDNT_FIT = 275,
        ERROR_EA_FILE_CORRUPT = 276,
        ERROR_EA_TABLE_FULL = 277,
        ERROR_INVALID_EA_HANDLE = 278,
        ERROR_EAS_NOT_SUPPORTED = 282,
        ERROR_NOT_OWNER = 288,
        ERROR_TOO_MANY_POSTS = 298,
        ERROR_PARTIAL_COPY = 299,
        ERROR_OPLOCK_NOT_GRANTED = 300,
        ERROR_INVALID_OPLOCK_PROTOCOL = 301,
        ERROR_DISK_TOO_FRAGMENTED = 302,
        ERROR_DELETE_PENDING = 303,
        ERROR_INCOMPATIBLE_WITH_GLOBAL_SHORT_NAME_REGISTRY_SETTING = 304,
        ERROR_SHORT_NAMES_NOT_ENABLED_ON_VOLUME = 305,
        ERROR_SECURITY_STREAM_IS_INCONSISTENT = 306,
        ERROR_INVALID_LOCK_RANGE = 307,
        ERROR_IMAGE_SUBSYSTEM_NOT_PRESENT = 308,
        ERROR_NOTIFICATION_GUID_ALREADY_DEFINED = 309,
        ERROR_INVALID_EXCEPTION_HANDLER = 310,
        ERROR_DUPLICATE_PRIVILEGES = 311,
        ERROR_NO_RANGES_PROCESSED = 312,
        ERROR_NOT_ALLOWED_ON_SYSTEM_FILE = 313,
        ERROR_DISK_RESOURCES_EXHAUSTED = 314,
        ERROR_INVALID_TOKEN = 315,
        ERROR_DEVICE_FEATURE_NOT_SUPPORTED = 316,
        ERROR_MR_MID_NOT_FOUND = 317,
        ERROR_SCOPE_NOT_FOUND = 318,
        ERROR_UNDEFINED_SCOPE = 319,
        ERROR_INVALID_CAP = 320,
        ERROR_DEVICE_UNREACHABLE = 321,
        ERROR_DEVICE_NO_RESOURCES = 322,
        ERROR_DATA_CHECKSUM_ERROR = 323,
        ERROR_INTERMIXED_KERNEL_EA_OPERATION = 324,
        ERROR_FILE_LEVEL_TRIM_NOT_SUPPORTED = 326,
        ERROR_OFFSET_ALIGNMENT_VIOLATION = 327,
        ERROR_INVALID_FIELD_IN_PARAMETER_LIST = 328,
        ERROR_OPERATION_IN_PROGRESS = 329,
        ERROR_BAD_DEVICE_PATH = 330,
        ERROR_TOO_MANY_DESCRIPTORS = 331,
        ERROR_SCRUB_DATA_DISABLED = 332,
        ERROR_NOT_REDUNDANT_STORAGE = 333,
        ERROR_RESIDENT_FILE_NOT_SUPPORTED = 334,
        ERROR_COMPRESSED_FILE_NOT_SUPPORTED = 335,
        ERROR_DIRECTORY_NOT_SUPPORTED = 336,
        ERROR_NOT_READ_FROM_COPY = 337,
        ERROR_FAIL_NOACTION_REBOOT = 350,
        ERROR_FAIL_SHUTDOWN = 351,
        ERROR_FAIL_RESTART = 352,
        ERROR_MAX_SESSIONS_REACHED = 353,
        ERROR_THREAD_MODE_ALREADY_BACKGROUND = 400,
        ERROR_THREAD_MODE_NOT_BACKGROUND = 401,
        ERROR_PROCESS_MODE_ALREADY_BACKGROUND = 402,
        ERROR_PROCESS_MODE_NOT_BACKGROUND = 403,
        ERROR_INVALID_ADDRESS = 487,
    }
}
