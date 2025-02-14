using ExeClient.Services;
using ExeClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;

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

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient<IProcessServiceClient, ProcessServiceRestClient>(client =>
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

        services.AddHostedService<ProcessStatusReporterService>();
    })
    .Build();

await host.RunAsync();
