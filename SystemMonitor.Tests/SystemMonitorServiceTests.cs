using Microsoft.Extensions.Logging;
using Moq;
using SystemMonitor.Core.Models;
using SystemMonitor.Core.Services;

namespace SystemMonitor.Tests
{
    public class SystemMonitorServiceTests
    {
        private readonly SystemMonitorService _service;

        public SystemMonitorServiceTests()
        {
            Mock<ILogger<SystemMonitorService>> loggerMock = new Mock<ILogger<SystemMonitorService>>();
            _service = new SystemMonitorService(loggerMock.Object);
        }

        [Fact]
        public async Task GetCurrentMetrics_ShouldReturnValidMetrics()
        {
            // Act
            var metrics = await _service.GetCurrentMetricsAsync();

            // Assert
            Assert.NotNull(metrics);
            Assert.InRange(metrics.CpuUsage, 0, 100);
            Assert.InRange(metrics.MemoryUsage, 0, 100);
            Assert.InRange(metrics.DiskUsage, 0, 100);
            Assert.NotNull(metrics.NetworkMetrics);
        }

        [Theory]
        [InlineData(95, 50, 50)] // Critical CPU
        [InlineData(50, 95, 50)] // Critical Memory
        [InlineData(50, 50, 95)] // Critical Disk
        public async Task IsResourceCritical_ShouldReturnTrue_WhenResourcesAreCritical(
            double cpu, double memory, double disk)
        {
            // Arrange
            var metrics = new SystemMetrics
            {
                CpuUsage = cpu,
                MemoryUsage = memory,
                DiskUsage = disk,
                NetworkMetrics = new NetworkMetrics(),
                Timestamp = DateTime.UtcNow
            };

            // Act
            var isCritical = await _service.IsResourceCriticalAsync(metrics);

            // Assert
            Assert.True(isCritical);
        }

        [Fact]
        public async Task IsResourceCritical_ShouldReturnFalse_WhenResourcesAreNormal()
        {
            // Arrange
            var metrics = new SystemMetrics
            {
                CpuUsage = 50,
                MemoryUsage = 50,
                DiskUsage = 50,
                NetworkMetrics = new NetworkMetrics(),
                Timestamp = DateTime.UtcNow
            };

            // Act
            var isCritical = await _service.IsResourceCriticalAsync(metrics);

            // Assert
            Assert.False(isCritical);
        }
    }
}