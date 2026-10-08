using ModelContextProtocol.Server;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

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

        [McpServerTool]
        [Description("Retorna el espacio usado y libre de la unidad donde está instalado el SO.")]
        public static string GetDiskSO_Space()
        {
            try
            {
                var systemRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "/";
                var drive = new DriveInfo(systemRoot);

                if (!drive.IsReady)
                {
                    return $"La unidad {drive.Name} no está lista.";
                }

                decimal total = drive.TotalSize;
                decimal free = drive.AvailableFreeSpace;
                decimal used = total - free;

                decimal totalGb = Math.Round(total / (1024m * 1024m * 1024m), 2);
                decimal freeGb = Math.Round(free / (1024m * 1024m * 1024m), 2);
                decimal usedGb = Math.Round(used / (1024m * 1024m * 1024m), 2);

                decimal usedPct = total == 0 ? 0 : Math.Round(used / total * 100, 1);

                return $"""
                Unidad: {drive.Name}
                Capacidad Total: {totalGb} GB
                Usado: {usedGb} GB ({usedPct} %)
                Libre: {freeGb} GB
                """;
            }
            catch (Exception ex)
            {
                return $"Error al obtener información del disco: {ex.Message}";
            }
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
