using ExeMonitoringShared;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ExeClient.Services;

public class ProcessServiceRestClient : IProcessServiceClient
{
    private readonly HttpClient _httpClient;

    public ProcessServiceRestClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProcessStatus>> GetStatusesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ProcessStatus>>("/api/process/statuses");
    }

    public async Task UpdateStatusAsync(ProcessStatus status)
    {
        await _httpClient.PostAsJsonAsync("/api/process/update", status);
    }
}

