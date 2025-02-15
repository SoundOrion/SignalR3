using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExeClient;

 public class HealthCheckHeartbeatService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IHealthCheckPublisher _healthCheckPublisher;
    private readonly ILogger<HealthCheckHeartbeatService> _logger;

    public HealthCheckHeartbeatService(
        HealthCheckService healthCheckService,
        IHealthCheckPublisher healthCheckPublisher,
        ILogger<HealthCheckHeartbeatService> logger)
    {
        _healthCheckService = healthCheckService;
        _healthCheckPublisher = healthCheckPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("✅ HealthCheckBackgroundService が開始されました");

        while (!stoppingToken.IsCancellationRequested)
        {
            var report = await _healthCheckService.CheckHealthAsync(stoppingToken);
            await _healthCheckPublisher.PublishAsync(report, stoppingToken);

            await Task.Delay(5000, stoppingToken); // ✅ `5秒ごとに PUSH`
        }
    }
}
