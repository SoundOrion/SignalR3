namespace ExeMonitoringServer.Services;

public interface IProcessService
{
    Task UpdateStatusAsync(ProcessStatus status);
    Task<List<ProcessStatus>> GetAllStatusesAsync();
}
