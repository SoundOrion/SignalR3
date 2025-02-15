using ExeClient.Services;
using ExeClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

//✅ リトライ(Retry)

//最大 3 回リトライ
//2 秒間隔の指数バックオフ
//HTTP 500 (InternalServerError) の場合のみリトライ
//✅ タイムアウト (Timeout)

//リクエストが 10 秒以上かかったらキャンセル
//✅ サーキットブレーカー (CircuitBreaker)

//30秒間のリクエストのうち、50% 以上が失敗するとブレーク
//最低 5 リクエスト以上で有効化
//ブレーク状態になったら 15 秒間リクエストを拒否

//var host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices((hostContext, services) =>
//    {
//        services.AddHttpClient<IProcessServiceClient, ProcessServiceRestClient>(client =>
//        {
//            client.BaseAddress = new Uri("http://localhost:5291");
//        })
//        .AddResilienceHandler("HttpResilience", builder =>
//        {
//            builder.AddRetry(new HttpRetryStrategyOptions
//            {
//                MaxRetryAttempts = 3, // 最大3回リトライ
//                Delay = TimeSpan.FromSeconds(2), // リトライ間隔
//                BackoffType = DelayBackoffType.Exponential, // 指数バックオフ
//                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
//            });

//            builder.AddTimeout(TimeSpan.FromSeconds(10)); // 10秒でタイムアウト

//            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
//            {
//                FailureRatio = 0.5, // 失敗率 50% 以上でブレーク
//                SamplingDuration = TimeSpan.FromSeconds(30), // 30秒間のリクエストを対象
//                MinimumThroughput = 5, // 最低 5 リクエストで判定
//                BreakDuration = TimeSpan.FromSeconds(15) // 15秒間ブレーク
//            });
//        });

//        services.AddHostedService<HeartbeatService>();

//    })
//    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient("HealthCheck", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5291");
        })
        .AddResilienceHandler("HttpResilience", builder =>
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3, // 最大3回リトライ
                Delay = TimeSpan.FromSeconds(2), // リトライ間隔
                BackoffType = DelayBackoffType.Exponential, // 指数バックオフ
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            });

            builder.AddTimeout(TimeSpan.FromSeconds(10)); // 10秒でタイムアウト

            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5, // 失敗率 50% 以上でブレーク
                SamplingDuration = TimeSpan.FromSeconds(30), // 30秒間のリクエストを対象
                MinimumThroughput = 5, // 最低 5 リクエストで判定
                BreakDuration = TimeSpan.FromSeconds(15) // 15秒間ブレーク
            });
        });

        services.AddHealthChecks() // ✅ HealthChecks を追加
          .AddCheck("ExampleCheck", () =>
              HealthCheckResult.Healthy("正常に動作しています"));

        services.AddSingleton<IHealthCheckPublisher, HealthCheckPublisher>(); // ✅ `IHealthCheckPublisher` を登録

        services.AddHostedService<HealthCheckHeartbeatService>();

    })
    .Build();

await host.RunAsync();










using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient("HealthCheck", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5291"); // ✅ `Push (ハートビート)` の送信先
        });

        services.AddHostedService<HttpServerService>(); // ✅ `ヘルスチェック API (/health)`
        services.AddHostedService<HeartbeatService>();  // ✅ `ハートビート送信 (Push)`
        services.AddHostedService<MainProcessingService>(); // ✅ `本来の処理`
    })
    .Build();

await host.RunAsync();


using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class HttpServerService : BackgroundService
{
    private readonly ILogger<HttpServerService> _logger;
    private readonly HttpListener _listener;
    private readonly string _url = "http://localhost:5001/";

    public HttpServerService(ILogger<HttpServerService> logger)
    {
        _logger = logger;
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener.Start();
        _logger.LogInformation("✅ HTTP ヘルスチェック API が開始されました: {Url}", _url);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }
            catch (Exception ex) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("🛑 HTTP サーバーが停止しました: {Message}", ex.Message);
                break;
            }
        }

        _listener.Stop();
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/health")
        {
            var healthData = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            };

            string responseJson = JsonSerializer.Serialize(healthData);
            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);

            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        response.OutputStream.Close();
    }
}



using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

public class HeartbeatService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly string _clientId = Guid.NewGuid().ToString();
    private readonly string _serverEndpoint = "/api/process/update";

    public HeartbeatService(IHttpClientFactory httpClientFactory, ILogger<HeartbeatService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("HealthCheck");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("✅ HeartbeatService が開始されました");

        while (!stoppingToken.IsCancellationRequested)
        {
            var status = new
            {
                ClientId = _clientId,
                IsRunning = true,
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                await _httpClient.PostAsJsonAsync(_serverEndpoint, status, stoppingToken);
                _logger.LogInformation("✅ Heartbeat (PUSH) を送信: {ClientId}", _clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Heartbeat (PUSH) の送信に失敗");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}




using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MainProcessingService : BackgroundService
{
    private readonly ILogger<MainProcessingService> _logger;

    public MainProcessingService(ILogger<MainProcessingService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("✅ メイン処理を開始します");

        while (!stoppingToken.IsCancellationRequested)
        {
            // ✅ ここに本来の処理を実装
            _logger.LogInformation("🔄 処理中...");
            await Task.Delay(10000, stoppingToken); // 例: 10秒ごとに何か処理
        }
    }
}
