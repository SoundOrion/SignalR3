using ExeMonitoringServer.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ExeMonitoringServer.Hubs;

public class ProcessHub : Hub
{
    private readonly IProcessService _processService;

    public ProcessHub(IProcessService processService)
    {
        _processService = processService;
    }

    public async Task GetLatestStatuses()
    {
        var statuses = await _processService.GetAllStatusesAsync();
        await Clients.All.SendAsync("ReceiveStatusUpdate", statuses);
    }
}

