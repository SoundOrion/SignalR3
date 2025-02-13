using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExeClient.Services;

public interface IProcessServiceClient
{
    Task<List<ProcessStatus>> GetStatusesAsync();
    Task UpdateStatusAsync(ProcessStatus status);
}
