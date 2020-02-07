using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BasicInjector
{
    internal class Injection
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);


        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;

        public bool InjectionPrep(string DLLLoc, int procID)
        {
            if (!File.Exists(DLLLoc))
            {
                System.Windows.MessageBox.Show("No DLL selected.", "ERROR");
                return false;
            }
            return this.StartInjection(Convert.ToUInt32(procID), DLLLoc);
        }

        public bool StartInjection(uint pid, string dllToInject)
        {
            IntPtr intPtr = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, 0, pid);
            if (intPtr == IntPtr.Zero)
            {
                return false;
            }
            IntPtr procAddress = Injection.GetProcAddress(Injection.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (procAddress == IntPtr.Zero)
            {
                return false;
            }
            IntPtr intPtr2 = Injection.VirtualAllocEx(intPtr, (IntPtr)null, (IntPtr)dllToInject.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (intPtr2 == IntPtr.Zero)
            {
                return false;
            }
            byte[] bytes = Encoding.ASCII.GetBytes(dllToInject);
            if (Injection.WriteProcessMemory(intPtr, intPtr2, bytes, (uint)bytes.Length, 0) == 0)
            {
                return false;
            }
            if (Injection.CreateRemoteThread(intPtr, (IntPtr)null, IntPtr.Zero, procAddress, intPtr2, 0u, (IntPtr)null) == IntPtr.Zero)
            {
                return false;
            }
            Injection.CloseHandle(intPtr);
            MainWindow._instance.AppendTextBox("Injection was successful");
            return true;
        }
    }
}
