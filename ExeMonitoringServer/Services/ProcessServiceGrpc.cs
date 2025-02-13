using ExeMonitoringServer;
using ExeMonitoringServer.Hubs;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ExeMonitoringServer.Services;

public class ProcessServiceGrpc : ProcessService.ProcessServiceBase, IProcessService
{
    private static ConcurrentDictionary<string, ProcessStatus> _processes = new();
    private readonly IHubContext<ProcessHub> _hubContext;

    public ProcessServiceGrpc(IHubContext<ProcessHub> hubContext)
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

    public override async Task<Empty> UpdateStatus(ProcessStatus request, ServerCallContext context)
    {
        await UpdateStatusAsync(request);
        return new Empty();
    }

    public override async Task<ProcessStatusList> GetAllStatuses(Empty request, ServerCallContext context)
    {
        var statuses = await GetAllStatusesAsync();
        return new ProcessStatusList { Statuses = { statuses } };
    }
}
