namespace ExeMonitoringShared;

public class ProcessStatus
{
    public string ClientId { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public DateTime LastUpdated { get; set; } // ✅ UI 側で使用する
}
