using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using BookingSystem_Explore.Infrastructure.Constants;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(Versions.Version1)]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BranchResponse>>> GetBranchesAsync(CancellationToken cancellationToken)
    {
        var res = await _branchService.GetBranchesAsync(cancellationToken);
        return Ok(res);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchResponse>> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var res = await _branchService.GetBranchByIdAsync(id, cancellationToken);
        if (res == null) return NotFound();
        return Ok(res);
    }

    [HttpGet("{id:guid}/services")]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetBranchServicesAsync(Guid id, CancellationToken cancellationToken)
    {
        var res = await _branchService.GetBranchServicesAsync(id, cancellationToken);
        return Ok(res);
    }
}
