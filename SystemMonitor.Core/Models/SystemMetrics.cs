namespace SystemMonitor.Core.Models
{
    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public NetworkMetrics? NetworkMetrics { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class NetworkMetrics
    {
        public double BytesSent { get; set; }
        public double BytesReceived { get; set; }
        public double CurrentBandwidth { get; set; }
    }
}