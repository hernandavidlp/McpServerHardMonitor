using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace McpServer1.Tools
{
    [McpServerToolType]
    public static class SystemMonitorTools
    {
        [McpServerTool]
        [Description("Retorna información de memoria RAM del sistema: total, usada y libre.")]
        public static string GetMemoryInfo()
        {
            var status = new MEMORYSTATUSEX();
            status.dwLength = (uint)Marshal.SizeOf(status);
            GlobalMemoryStatusEx(ref status);

            long totalGb = (long)status.ullTotalPhys / (1024 * 1024 * 1024);
            long freeGb = (long)status.ullAvailPhys / (1024 * 1024 * 1024);
            long usedGb = totalGb - freeGb;

            return $"""
            RAM Total: {totalGb} GB
            RAM Usada: {usedGb} GB
            RAM Libre:  {freeGb} GB
            Uso: {status.dwMemoryLoad}%
            """;
        }

        [McpServerTool]
        [Description("Retorna el tiempo que lleva encendida la máquina.")]
        public static string GetUptime()
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

            return $"El sistema lleva encendido: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }

        // --- P/Invoke para Windows API ---

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }
    }
}
