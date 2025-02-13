using ExeMonitoringServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExeMonitoringServer.Controllers;

[ApiController]
[Route("api/process")]
public class ProcessController : ControllerBase
{
    private readonly IProcessService _processService;

    public ProcessController(IProcessService processService)
    {
        _processService = processService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateStatus([FromBody] ProcessStatus status)
    {
        await _processService.UpdateStatusAsync(status);
        return Ok();
    }

    [HttpGet("statuses")]
    public async Task<IEnumerable<ProcessStatus>> GetStatuses()
    {
        return await _processService.GetAllStatusesAsync();
    }
}
