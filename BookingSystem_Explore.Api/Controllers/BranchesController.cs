using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
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
    public async Task<ActionResult<IReadOnlyList<BranchResponse>>> GetBranchesAsync(CancellationToken cancellationToken, [FromQuery] bool includeInactive = false)
    {
        var res = await _branchService.GetBranchesAsync(cancellationToken, includeInactive);
        return Ok(res);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchResponse>> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var res = await _branchService.GetBranchByIdAsync(id, cancellationToken);
        if (res == null) return NotFound();
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<BranchResponse>> AddBranchAsync([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var created = await _branchService.AddBranchAsync(request, cancellationToken);
        var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1";
        return CreatedAtAction("GetBranchById", new { id = created.BranchId, version }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BranchResponse>> UpdateBranchAsync(Guid id, [FromBody] UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        var updated = await _branchService.UpdateBranchAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet("{id:guid}/services")]
    public async Task<ActionResult<IReadOnlyList<BranchServiceResponse>>> GetBranchServicesAsync(Guid id, CancellationToken cancellationToken)
    {
        var res = await _branchService.GetBranchServicesAsync(id, cancellationToken);
        return Ok(res);
    }

    [HttpPost("{id:guid}/services")]
    public async Task<ActionResult<BranchServiceResponse>> AddBranchServiceAsync(Guid id, [FromBody] CreateBranchServiceRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var created = await _branchService.AddBranchServiceAsync(id, request, cancellationToken);
        var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1";
        return CreatedAtAction("GetBranchServices", new { id, version }, created);
    }

    [HttpPut("{id:guid}/services/{branchServiceId:guid}")]
    public async Task<ActionResult<BranchServiceResponse>> UpdateBranchServiceAsync(Guid id, Guid branchServiceId, [FromBody] UpdateBranchServiceRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var updated = await _branchService.UpdateBranchServiceAsync(id, branchServiceId, request, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
