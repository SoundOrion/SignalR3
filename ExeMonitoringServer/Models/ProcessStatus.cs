namespace ExeMonitoringServer.Models;

public class ProcessStatus
{
    public string ClientId { get; set; }
    public bool IsRunning { get; set; }
    public string Timestamp { get; set; }
}