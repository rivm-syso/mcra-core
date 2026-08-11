using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MCRA.Utils.PerformanceReporting {
    public record MemoryMetrics {
        public ulong Total;
        public ulong Used;
        public ulong Free;
    }

    public static class PerformanceInfo {

        /// <summary>
        /// Returns the current memory useage in GB, MB, KB, Bytes
        /// </summary>
        /// <returns></returns>
        public static string GetMemoryUsage() {
            var proc = Process.GetCurrentProcess();
            var bytes = proc.PrivateMemorySize64;
            const int scale = 1024;
            var orders = new string[] { "GB", "MB", "KB", "Bytes" };
            var max = (long)Math.Pow(scale, orders.Length - 1);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var order in orders) {
                if (bytes > max) {
                    return $"----- Memory usage: {decimal.Divide(bytes, max):##.##} {order}";
                }
                max /= scale;
            }
            return "0 Bytes";
        }

        /// <summary>
        /// Returns the current peak memory useage in GB, MB, KB, Bytes
        /// </summary>
        /// <returns></returns>
        public static string GetPeakMemoryUsage() {
            var proc = Process.GetCurrentProcess();
            var bytes = proc.PeakWorkingSet64;
            const int scale = 1024;
            var orders = new string[] { "GB", "MB", "KB", "Bytes" };
            var max = (long)Math.Pow(scale, orders.Length - 1);
            foreach (var order in orders) {
                if (bytes > max) {
                    return $"----- Peak memory usage: {Decimal.Divide(bytes, max):##.##} {order}";
                }
                max /= scale;
            }
            return "0 Bytes";
        }

        public static MemoryMetrics GetMemoryMetrics() => isUnix ? getUnixMetrics() : getWindowsMetrics();

        private static bool isUnix => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static ulong GetTotalMemory() => GetMemoryMetrics().Total;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MemoryStatusEx {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MemoryStatusEx() {
                dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>();
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

        private static MemoryMetrics getWindowsMetrics() {
            var status = new MemoryStatusEx();
            if (!GlobalMemoryStatusEx(status)) {
                return new MemoryMetrics();
            }

            var metrics = new MemoryMetrics {
                Total = status.ullTotalPhys / 1024 / 1024,
                Free = status.ullAvailPhys / 1024 / 1024
            };
            metrics.Used = metrics.Total - metrics.Free;

            return metrics;
        }

        private static MemoryMetrics getUnixMetrics() {
            var output = "";

            var info = new ProcessStartInfo("free -m") {
                FileName = "/bin/bash",
                Arguments = "-c \"free -m\"",
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info)) {
                if (process == null) {
                    return new MemoryMetrics();
                }
                output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var metrics = new MemoryMetrics {
                Total = ulong.Parse(memory[1]),
                Used = ulong.Parse(memory[2]),
                Free = ulong.Parse(memory[3])
            };

            return metrics;
        }
    }
}
