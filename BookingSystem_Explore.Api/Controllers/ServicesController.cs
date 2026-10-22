using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using BookingSystem_Explore.Infrastructure.Constants;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(Versions.Version1)]
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponse>>> GetServicesAsync(CancellationToken cancellationToken)
    {
        var services = await _serviceService.GetServicesAsync(cancellationToken);
        return Ok(services);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> GetServiceByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var service = await _serviceService.GetServiceByIdAsync(id, cancellationToken);
        if (service == null) return NotFound();
        return Ok(service);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> AddServiceAsync([FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var created = await _serviceService.AddServiceAsync(request, cancellationToken);
        var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1";
        return CreatedAtAction(nameof(GetServiceByIdAsync), new { id = created.ServiceId, version }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> UpdateServiceAsync(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var updated = await _serviceService.UpdateServiceAsync(id, request, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
