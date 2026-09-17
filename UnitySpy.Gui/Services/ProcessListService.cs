using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HackF5.UnitySpy.Gui.Services
{
    public sealed class ProcessItem
    {
        public ProcessItem(string name, int id, bool isHearthstone, bool hasMono, bool? is64Bit)
        {
            this.Name = name;
            this.Id = id;
            this.IsHearthstone = isHearthstone;
            this.HasMono = hasMono;
            this.Is64Bit = is64Bit;
        }

        public string Name { get; }

        public int Id { get; }

        public bool IsHearthstone { get; }

        public bool HasMono { get; }

        public bool? Is64Bit { get; }

        public string DisplayName
        {
            get
            {
                var bitness = this.Is64Bit == true ? "x64" : this.Is64Bit == false ? "x86" : "?";
                var flags = this.IsHearthstone ? "Hearthstone" : this.HasMono ? "Unity" : null;
                return flags == null
                    ? $"{this.Name} ({this.Id}) [{bitness}]"
                    : $"{this.Name} ({this.Id}) [{bitness}, {flags}]";
            }
        }
    }

    public static class ProcessListService
    {
        private const string MonoModule = "mono-2.0-bdwgc";
        private const uint ListModulesAll = 0x03;

        public static IReadOnlyList<ProcessItem> ListProcesses()
        {
            var items = new List<ProcessItem>();
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(process.ProcessName))
                    {
                        continue;
                    }

                    var isHearthstone = process.ProcessName.Equals("Hearthstone", StringComparison.OrdinalIgnoreCase);
                    var hasMono = false;
                    bool? is64Bit = null;
                    try
                    {
                        is64Bit = Is64BitProcess(process);
                        if (isHearthstone
                            || process.ProcessName.IndexOf("Unity", StringComparison.OrdinalIgnoreCase) >= 0
                            || process.MainWindowHandle != IntPtr.Zero)
                        {
                            hasMono = HasMonoModule(process);
                        }
                    }
                    catch
                    {
                        // Access denied or bitness mismatch — still list the process.
                    }

                    items.Add(new ProcessItem(process.ProcessName, process.Id, isHearthstone, hasMono, is64Bit));
                }
                catch
                {
                    // Skip processes we cannot inspect at all.
                }
                finally
                {
                    process.Dispose();
                }
            }

            return items
                .OrderByDescending(p => p.IsHearthstone)
                .ThenByDescending(p => p.HasMono)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Id)
                .ToList();
        }

        public static bool IsCurrentProcess64Bit() => Environment.Is64BitProcess;

        public static bool BitnessMatches(ProcessItem process)
        {
            if (process.Is64Bit == null)
            {
                return true;
            }

            return process.Is64Bit.Value == Environment.Is64BitProcess;
        }

        private static bool HasMonoModule(Process process)
        {
            var modules = GetModulePointers(process.Handle);
            var name = new StringBuilder(1024);
            foreach (var module in modules)
            {
                name.Clear();
                if (GetModuleFileNameEx(process.Handle, module, name, (uint)name.Capacity) == 0)
                {
                    continue;
                }

                if (name.ToString().IndexOf(MonoModule, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Is64BitProcess(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                return false;
            }

            if (!IsWow64Process(process.Handle, out var wow64))
            {
                throw new InvalidOperationException("IsWow64Process failed.");
            }

            return !wow64;
        }

        private static IntPtr[] GetModulePointers(IntPtr processHandle)
        {
            var modulePointers = new IntPtr[2048];
            if (!EnumProcessModulesEx(processHandle, modulePointers, modulePointers.Length * IntPtr.Size, out var bytesNeeded, ListModulesAll))
            {
                return Array.Empty<IntPtr>();
            }

            var count = bytesNeeded / IntPtr.Size;
            if (count <= 0)
            {
                return Array.Empty<IntPtr>();
            }

            var result = new IntPtr[Math.Min(count, modulePointers.Length)];
            Array.Copy(modulePointers, result, result.Length);
            return result;
        }

        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [Out] IntPtr[] lphModule,
            int cb,
            out int lpcbNeeded,
            uint dwFilterFlag);

        [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpFilename, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);
    }
}
