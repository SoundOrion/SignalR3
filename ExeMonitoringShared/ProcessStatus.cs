using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ExeMonitoringShared;

public class ProcessStatus
{
    public string ClientId { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public DateTime LastUpdated { get; set; } // ✅ UI 側で使用する
}

public class HealthStatus
{
    public string ClientId { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public string Status { get; set; } = "Unknown";
    public Dictionary<string, HealthReportEntry> Results { get; set; } = new();
}