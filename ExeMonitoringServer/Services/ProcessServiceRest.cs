using ExeMonitoringServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ExeMonitoringServer.Services;

public class ProcessServiceRest : IProcessService
{
    private static ConcurrentDictionary<string, ProcessStatus> _processes = new();
    private readonly IHubContext<ProcessHub> _hubContext;

    public ProcessServiceRest(IHubContext<ProcessHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task UpdateStatusAsync(ProcessStatus status)
    {
        _processes[status.ClientId] = status;
        await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", _processes.Values);
    }

    public async Task<List<ProcessStatus>> GetAllStatusesAsync()
    {
        return _processes.Values.ToList();
    }
}
