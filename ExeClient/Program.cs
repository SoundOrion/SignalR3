using Grpc.Net.Client;
using ExeClient.Services;

class Program
{
    private static readonly string clientId = Guid.NewGuid().ToString();
    private static IProcessServiceClient _processServiceClientGrpc;
    private static IProcessServiceClient _processServiceClientRest;

    static async Task Main()
    {
        Console.WriteLine("=== EXE クライアント 起動 ===");

        bool useGrpc = false;
        bool useRest = true;

        // gRPC クライアント
        var grpcChannel = GrpcChannel.ForAddress("http://localhost:5291");
        _processServiceClientGrpc = new ProcessServiceGrpcClient(grpcChannel);

        // REST API クライアント
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5291") };
        _processServiceClientRest = new ProcessServiceRestClient(httpClient);

        var tasks = new List<Task>();

        if (useGrpc)
        {
            Console.WriteLine("[INFO] gRPC で状態を送信します。");
            tasks.Add(SendStatusLoop(_processServiceClientGrpc));
        }

        if (useRest)
        {
            Console.WriteLine("[INFO] REST API で状態を送信します。");
            tasks.Add(SendStatusLoop(_processServiceClientRest));
        }

        if (tasks.Count == 0)
        {
            Console.WriteLine("[ERROR] gRPC または REST API を有効にしてください（環境変数: USE_GRPC / USE_REST）");
            return;
        }

        await Task.WhenAll(tasks);
    }

    static async Task SendStatusLoop(IProcessServiceClient serviceClient)
    {
        while (true)
        {
            var status = new ProcessStatus
            {
                ClientId = clientId,
                IsRunning = true,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            try
            {
                await serviceClient.UpdateStatusAsync(status);
                Console.WriteLine($"[{clientId}] 状態を送信しました ({serviceClient.GetType().Name})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{clientId}] エラー: {ex.Message} ({serviceClient.GetType().Name})");
            }

            await Task.Delay(5000);
        }
    }
}
