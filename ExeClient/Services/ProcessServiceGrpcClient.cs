using Grpc.Net.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExeClient.Services;

public class ProcessServiceGrpcClient : IProcessServiceClient
{
    private readonly ProcessService.ProcessServiceClient _client;

    public ProcessServiceGrpcClient(GrpcChannel channel)
    {
        _client = new ProcessService.ProcessServiceClient(channel);
    }

    public async Task<List<ProcessStatus>> GetStatusesAsync()
    {
        var response = await _client.GetAllStatusesAsync(new Empty());
        return new List<ProcessStatus>(response.Statuses);
    }

    public async Task UpdateStatusAsync(ProcessStatus status)
    {
        await _client.UpdateStatusAsync(status);
    }
}
