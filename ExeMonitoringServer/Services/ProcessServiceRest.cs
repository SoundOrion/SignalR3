using ExeMonitoringShared;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ExeMonitoringServer.Services;

public class ProcessServiceRest : IProcessService
{
    private readonly IHubContext<ProcessHub> _hubContext;

    public ProcessServiceRest(IHubContext<ProcessHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task UpdateStatusAsync(ProcessStatus status)
    {
        // ✅ `ProcessHub` の `_processes` にデータを保存
        ProcessHub.UpdateProcessStatus(status);

        // ✅ 正しいイベント名 `ReceiveStatusUpdate` を使って送信
        await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", ProcessHub.GetAllProcessStatuses());
    }

    public async Task<List<ProcessStatus>> GetAllStatusesAsync()
    {
        return ProcessHub.GetAllProcessStatuses();
    }
}
