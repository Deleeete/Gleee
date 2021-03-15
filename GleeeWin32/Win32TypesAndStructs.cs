using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Gleee.Win32
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }
    public enum MemoryState : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_FREE = 0x10000,
        MEM_RESERVE = 0x2000
    }
    public enum AllocationType : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_RESERVE = 0x2000,
        MEM_RESET = 0x8000,
        MEM_RESET_UNDO = 0x1000000,
        MEM_LARGE_PAGES = 0x20000000,
        MEM_PHYSICAL = 0x00400000,
        MEM_WRITE_WATCH = 0x00200000,
        MEM_TOP_DOWN = 0x00100000,
    }
    public enum PageType : uint
    {
        MEM_IMAGE = 0x1000000,
        MEM_MAPPED = 0x40000,
        MEM_PRIVATE = 0x20000
    }
    public enum PageProtection : uint
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0X20,
        PAGE_EXECUTE_READWRITE = 0X40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0X01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0X04,
        PAGE_WRITECOPY = 0X08,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0X400,
    }
    public enum StartupFlags
    {
        STARTUP_CONCURRENT_GC = 0x1,
        STARTUP_LOADER_OPTIMIZATION_MASK = 0x3 << 1,
        STARTUP_LOADER_OPTIMIZATION_SINGLE_DOMAIN = 0x1 << 1,
        STARTUP_LOADER_OPTIMIZATION_MULTI_DOMAIN = 0x2 << 1,
        STARTUP_LOADER_OPTIMIZATION_MULTI_DOMAIN_HOST = 0x3 << 1,

        STARTUP_LOADER_SAFEMODE = 0x10,
        STARTUP_LOADER_SETPREFERENCE = 0x100,

        STARTUP_SERVER_GC = 0x1000,
        STARTUP_HOARD_GC_VM = 0x2000,

        STARTUP_SINGLE_VERSION_HOSTING_INTERFACE = 0x4000,
        STARTUP_LEGACY_IMPERSONATION = 0x10000,
        STARTUP_DISABLE_COMMITTHREADSTACK = 0x20000,
        STARTUP_ALWAYSFLOW_IMPERSONATION = 0x40000,
        STARTUP_TRIM_GC_COMMIT = 0x80000,

        STARTUP_ETW = 0x100000,
        STARTUP_ARM = 0x400000
    }
    public enum LoadLibraryFlags : uint
    {
        None = 0,
        DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
        LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
        LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
        LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
        LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
        LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
        LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
        LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
        LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left, Top, Right, Bottom;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ModuleInfo
    {
        public IntPtr lpBaseOfDll;
        public int SizeOfImage;
        public IntPtr EntryPoint;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemInfo
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public IntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInfomation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public AllocationType AllocationProtect;
        public IntPtr RegionSize;
        public MemoryState State;
        public PageProtection Protect;
        public PageType Type;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Win32_GUID
    {          
        public uint Data1;
        public ushort Data2;
        public ushort Data3;
        public byte Data4_0;
        public byte Data4_1;
        public byte Data4_2;
        public byte Data4_3;
        public byte Data4_4;
        public byte Data4_5;
        public byte Data4_6;
        public byte Data4_7;

        public Win32_GUID(uint data1, ushort data2, ushort data3, byte[] data4)
        {
            if (data4.Length != 4) throw new Exception("data4的长度必须为4");
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            Data4_0 = data4[0];
            Data4_1 = data4[1]; 
            Data4_2 = data4[2];
            Data4_3 = data4[3];
            Data4_4 = data4[4];
            Data4_5 = data4[5];
            Data4_6 = data4[6];
            Data4_7 = data4[7];
        }
        public Win32_GUID(uint data1, ushort data2, ushort data3, byte data4_0, byte data4_1, byte data4_2, byte data4_3, byte data4_4, byte data4_5, byte data4_6, byte data4_7)
        {
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            Data4_0 = data4_0;
            Data4_1 = data4_1;
            Data4_2 = data4_2;
            Data4_3 = data4_3;
            Data4_4 = data4_4;
            Data4_5 = data4_5;
            Data4_6 = data4_6;
            Data4_7 = data4_7;
        }
        public override string ToString()
        {
            return $"{{{Data1:x8}-{Data2:x4}-{Data3:x4}-{Data4_0:x2}{Data4_1:x2}{Data4_2:x2}{Data4_3:x2}{Data4_4:x2}{Data4_5:x2}{Data4_6:x2}{Data4_7:x2}}}";
        }
        public byte[] ToByteArray()
        {
            List<byte> b = new List<byte>();
            b.AddRange(BitConverter.GetBytes(Data1));
            b.AddRange(BitConverter.GetBytes(Data2));
            b.AddRange(BitConverter.GetBytes(Data3));
            b.AddRange(new byte[] 
            {
                Data4_0,
                Data4_1,
                Data4_2,
                Data4_3,
                Data4_4,
                Data4_5,
                Data4_6,
                Data4_7,
            });
            return b.ToArray();
        }

        public static Win32_GUID CLSID_CLRRuntimeHost = new Win32_GUID(0x90F1A06E, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
        public static Win32_GUID IID_ICLRRuntimeHost = new Win32_GUID(0x90F1A06C, 0x7712, 0x4762, 0x86, 0xB5, 0x7A, 0x5E, 0xBA, 0x6B, 0xDB, 0x02);
        public static Win32_GUID CLSID_CLRStrongName = new Win32_GUID(0xB79B0ACD, 0xF5CD, 0x409b, 0xB5, 0xA5, 0xA1, 0x62, 0x44, 0x61, 0x0B, 0x92);
        public static Win32_GUID IID_ICLRMetaHost = new Win32_GUID(0xD332DB9E, 0xB9B3, 0x4125, 0x82, 0x07, 0xA1, 0x48, 0x84, 0xF5, 0x32, 0x16);
        public static Win32_GUID CLSID_CLRMetaHost = new Win32_GUID(0x9280188d, 0xe8e, 0x4867, 0xb3, 0xc, 0x7f, 0xa8, 0x38, 0x84, 0xe8, 0xde);
        public static Win32_GUID IID_ICLRMetaHostPolicy = new Win32_GUID(0xE2190695, 0x77B2, 0x492e, 0x8E, 0x14, 0xC4, 0xB3, 0xA7, 0xFD, 0xD5, 0x93);
        public static Win32_GUID CLSID_CLRMetaHostPolicy = new Win32_GUID(0x2ebcd49a, 0x1b47, 0x4a61, 0xb1, 0x3a, 0x4a, 0x3, 0x70, 0x1e, 0x59, 0x4b);
        public static Win32_GUID IID_ICLRDebugging = new Win32_GUID(0xd28f3c5a, 0x9634, 0x4206, 0xa5, 0x9, 0x47, 0x75, 0x52, 0xee, 0xfb, 0x10);
        public static Win32_GUID CLSID_CLRDebugging = new Win32_GUID(0xbacc578d, 0xfbdd, 0x48a4, 0x96, 0x9f, 0x2, 0xd9, 0x32, 0xb7, 0x46, 0x34);
        public static Win32_GUID IID_ICLRRuntimeInfo = new Win32_GUID(0xBD39D1D2, 0xBA2F, 0x486a, 0x89, 0xB0, 0xB4, 0xB0, 0xCB, 0x46, 0x68, 0x91);
        public static Win32_GUID IID_ICLRStrongName = new Win32_GUID(0x9FD93CCF, 0x3280, 0x4391, 0xB3, 0xA9, 0x96, 0xE1, 0xCD, 0xE7, 0x7C, 0x8D);
        public static Win32_GUID IID_ICLRStrongName2 = new Win32_GUID(0xC22ED5C5, 0x4B59, 0x4975, 0x90, 0xEB, 0x85, 0xEA, 0x55, 0xC0, 0x06, 0x9B);
        public static Win32_GUID IID_ICLRStrongName3 = new Win32_GUID(0x22c7089b, 0xbbd3, 0x414a, 0xb6, 0x98, 0x21, 0x0f, 0x26, 0x3f, 0x1f, 0xed);
        public static Win32_GUID CLSID_CLRDebuggingLegacy = new Win32_GUID(0xDF8395B5, 0xA4BA, 0x450b, 0xA7, 0x7C, 0xA9, 0xA4, 0x77, 0x62, 0xC5, 0x20);
        public static Win32_GUID CLSID_CLRProfiling = new Win32_GUID(0xbd097ed8, 0x733e, 0x43fe, 0x8e, 0xd7, 0xa9, 0x5f, 0xf9, 0xa8, 0x44, 0x8c);
        public static Win32_GUID IID_ICLRProfiling = new Win32_GUID(0xb349abe3, 0xb56f, 0x4689, 0xbf, 0xcd, 0x76, 0xbf, 0x39, 0xd8, 0x88, 0xea);
        public static Win32_GUID IID_ICLRDebuggingLibraryProvider = new Win32_GUID(0x3151c08d, 0x4d09, 0x4f9b, 0x88, 0x38, 0x28, 0x80, 0xbf, 0x18, 0xfe, 0x51);

    }
    /// <summary>
    /// 指针的指针，用于输出指针的函数
    /// </summary>
    public class Ptr
    {
        public int Value;
        public Ptr(int value)
        {
            this.Value =  value;
        }

    }
}