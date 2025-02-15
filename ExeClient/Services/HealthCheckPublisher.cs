using ExeMonitoringShared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExeClient.Services;

public class HealthCheckPublisher : IHealthCheckPublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthCheckPublisher> _logger;
    private readonly string _serverEndpoint = "/api/process/update";
    private readonly string _clientId = Guid.NewGuid().ToString();

    public HealthCheckPublisher(IHttpClientFactory httpClientFactory, ILogger<HealthCheckPublisher> logger)
    {
        _httpClient = httpClientFactory.CreateClient("HealthCheck");
        _logger = logger;
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        //var healthStatus = new
        //{
        //    ClientId = _clientId,
        //    Timestamp = DateTime.UtcNow,
        //    Status = report.Status.ToString(),
        //    Results = report.Entries
        //};

        var healthStatus = new ProcessStatus
        {
            ClientId = _clientId,
            IsRunning = true,
            LastUpdated = DateTime.UtcNow
        };

        try
        {
            await _httpClient.PostAsJsonAsync(_serverEndpoint, healthStatus, cancellationToken);
            _logger.LogInformation("✅ ハートビート (PUSH) を送信: {0}", JsonSerializer.Serialize(healthStatus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ハートビート (PUSH) の送信に失敗");
        }
    }
}
