using ExeMonitoringShared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ProcessHub : Hub
{
    private static readonly ConcurrentDictionary<string, ProcessStatus> _processes = new();
    private static Timer? _monitorTimer;
    private static IHubContext<ProcessHub>? _hubContext;

    public ProcessHub(IHubContext<ProcessHub> hubContext)
    {
        _hubContext = hubContext;

        if (_monitorTimer == null)
        {
            _monitorTimer = new Timer(CheckClientHealth, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }
    }

    public async Task UpdateStatus(ProcessStatus status)
    {
        UpdateProcessStatus(status);
        await _hubContext!.Clients.All.SendAsync("ReceiveStatusUpdate", _processes.Values.ToList());
    }

    public static void UpdateProcessStatus(ProcessStatus status)
    {
        status.LastUpdated = DateTime.UtcNow;
        _processes[status.ClientId] = status;
    }

    private void CheckClientHealth(object? state)
    {
        var now = DateTime.UtcNow;
        bool updated = false;

        foreach (var (clientId, process) in _processes.ToList())
        {
            if ((now - process.LastUpdated).TotalSeconds > 30)
            {
                Console.WriteLine($"[WARNING] {clientId} が応答なし (30秒間ハートビートなし)");
                process.IsRunning = false;
                _processes[clientId] = process;
                updated = true;
            }
        }

        if (updated)
        {
            // ✅ `IHubContext` を利用し、適切に `Clients.All.SendAsync()` を呼ぶ
            _hubContext!.Clients.All.SendAsync("ReceiveStatusUpdate", _processes.Values.ToList());
        }
    }

    public static List<ProcessStatus> GetAllProcessStatuses()
    {
        return _processes.Values.ToList();
    }
}
