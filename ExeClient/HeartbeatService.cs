using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ExeClient.Services;
using ExeMonitoringShared;

namespace ExeClient;

public class HeartbeatService : BackgroundService
{
    private readonly ILogger<HeartbeatService> _logger;
    private readonly IProcessServiceClient _serviceClient;
    private readonly string _clientId = Guid.NewGuid().ToString();

    public HeartbeatService(IProcessServiceClient serviceClient, ILogger<HeartbeatService> logger)
    {
        _serviceClient = serviceClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProcessStatusReporterService が開始されました。");

        while (!stoppingToken.IsCancellationRequested)
        {
            var status = new ProcessStatus
            {
                ClientId = _clientId,
                IsRunning = true,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                await _serviceClient.UpdateStatusAsync(status);
                _logger.LogInformation("[{ClientId}] 状態を送信しました ({ServiceType})", _clientId, _serviceClient.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ClientId}] 状態送信エラー: {Message}", _clientId, ex.Message);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
