using System;
using System.Runtime.InteropServices;

namespace SimpleProgControl {
  public class Win32Kernel {

     [DllImport("kernel32.dll")]
     public static extern IntPtr OpenProcess(
          ProcessAccessFlags processAccess,
          bool bInheritHandle,
          uint processId
     );

     [Flags]
     public enum ProcessAccessFlags : uint {
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

     [DllImport("kernel32.dll", SetLastError = true)]
     public static extern UIntPtr VirtualAlloc(UIntPtr lpAddress, UIntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

     [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
     public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

     [Flags()]
     public enum AllocationType : uint {
        COMMIT = 0x1000,
        RESERVE = 0x2000,
        RESET = 0x80000,
        LARGE_PAGES = 0x20000000,
        PHYSICAL = 0x400000,
        TOP_DOWN = 0x100000,
        WRITE_WATCH = 0x200000
     }

     [Flags()]
     public enum MemoryProtection : uint {
        EXECUTE = 0x10,
        EXECUTE_READ = 0x20,
        EXECUTE_READWRITE = 0x40,
        EXECUTE_WRITECOPY = 0x80,
        NOACCESS = 0x01,
        READONLY = 0x02,
        READWRITE = 0x04,
        WRITECOPY = 0x08,
        GUARD_Modifierflag = 0x100,
        NOCACHE_Modifierflag = 0x200,
        WRITECOMBINE_Modifierflag = 0x400
     }

     [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
     public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

     //[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
     //static unsafe extern bool VirtualFreeEx(IntPtr hProcess, byte* pAddress, int size, FreeType freeType);

     [Flags]
     public enum FreeType {
        Decommit = 0x4000,
        Release = 0x8000,
     }

     [DllImport("kernel32.dll", SetLastError = true)]
     [return: MarshalAs(UnmanagedType.Bool)]
     public static extern bool CloseHandle(IntPtr hObject);

     [DllImport("kernel32.dll", SetLastError = true)]
     public static extern bool ReadProcessMemory(
         IntPtr hProcess,
         IntPtr lpBaseAddress,
         [Out] byte[] lpBuffer,
         int dwSize,
         out IntPtr lpNumberOfBytesRead);

     [DllImport("kernel32.dll", SetLastError = true)]
     public static extern bool ReadProcessMemory(
         IntPtr hProcess,
         IntPtr lpBaseAddress,
         [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
         int dwSize,
         out IntPtr lpNumberOfBytesRead);

     [DllImport("kernel32.dll", SetLastError = true)]
     public static extern bool ReadProcessMemory(
         IntPtr hProcess,
         IntPtr lpBaseAddress,
         IntPtr lpBuffer,
         int dwSize,
         out IntPtr lpNumberOfBytesRead);

   }
}
