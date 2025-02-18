using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services
{
    public interface ISystemMonitorService
    {
        Task<SystemMetrics> GetCurrentMetricsAsync();
        Task<bool> IsResourceCriticalAsync(SystemMetrics metrics);
        Task SaveMetricsAsync(SystemMetrics metrics);
    }

    public class SystemMonitorService(ILogger<SystemMonitorService> logger) : ISystemMonitorService
    {
        private readonly PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
        private readonly PerformanceCounter _ramCounter = new("Memory", "% Committed Bytes In Use");

        public async Task<SystemMetrics> GetCurrentMetricsAsync()
        {
            try
            {
                var metrics = new SystemMetrics
                {
                    
                    MemoryUsage = _ramCounter.NextValue(),
                    CpuUsage = _cpuCounter.NextValue(),
                    DiskUsage = GetDiskUsage(),
                    NetworkMetrics = await GetNetworkMetricsAsync(),
                    Timestamp = DateTime.UtcNow
                };

                return metrics;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error collecting system metrics");
                throw;
            }
        }

        private double GetDiskUsage()
        {
            var drive = DriveInfo.GetDrives().First();
            var totalSize = drive.TotalSize;
            var freeSpace = drive.AvailableFreeSpace;
            return ((double)(totalSize - freeSpace) / totalSize) * 100;
        }

        private Task<NetworkMetrics> GetNetworkMetricsAsync()
        {
            // Використовуємо NetworkInterface для отримання мережевої статистики
            var networkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up);

            if (networkInterface != null)
            {
                var stats = networkInterface.GetIPv4Statistics();
                return Task.FromResult(new NetworkMetrics
                {
                    BytesSent = stats.BytesSent,
                    BytesReceived = stats.BytesReceived,
                    CurrentBandwidth = networkInterface.Speed
                });
            }

            return Task.FromResult(new NetworkMetrics());
        }

        public Task<bool> IsResourceCriticalAsync(SystemMetrics metrics)
        {
            const double criticalCpuThreshold = 90.0;
            const double criticalMemoryThreshold = 90.0;
            const double criticalDiskThreshold = 90.0;

            return Task.FromResult(metrics.CpuUsage > criticalCpuThreshold ||
                                   metrics.MemoryUsage > criticalMemoryThreshold ||
                                   metrics.DiskUsage > criticalDiskThreshold);
        }

        public Task SaveMetricsAsync(SystemMetrics metrics)
        {
            logger.LogInformation($"Metrics saved: CPU: {metrics.CpuUsage}%, Memory: {metrics.MemoryUsage}%, Disk: {metrics.DiskUsage}%");
            return Task.CompletedTask;
        }
    }
}