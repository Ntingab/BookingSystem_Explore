using Asp.Versioning;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/appointment-slots")]
[ApiVersion(Versions.Version1)]
public class AppointmentSlotsController : ControllerBase
{
    private readonly IAppointmentSlotService _slotService;

    public AppointmentSlotsController(IAppointmentSlotService slotService)
    {
        _slotService = slotService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentSlotResponse>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IReadOnlyList<AppointmentSlotResponse>>> GetAvailableSlotsAsync([FromQuery][System.ComponentModel.DataAnnotations.Required] Guid branchServiceId, [FromQuery][System.ComponentModel.DataAnnotations.Required] string date, CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(date, out var d)) return BadRequest("Invalid date format. Use yyyy-MM-dd.");
        var res = await _slotService.GetAvailableSlotsAsync(branchServiceId, d, cancellationToken);
        return Ok(res);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentSlotResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AppointmentSlotResponse>> GetSlotByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var slot = await _slotService.GetSlotByIdAsync(id, cancellationToken);
        if (slot == null) return NotFound();
        return Ok(slot);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentSlotResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AppointmentSlotResponse>> CreateSlotAsync([FromBody] CreateAppointmentSlotRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var created = await _slotService.CreateSlotAsync(request, cancellationToken);
        // include api version route value so CreatedAtAction can resolve the route
        var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1";
        return CreatedAtAction(nameof(GetSlotByIdAsync), new { id = created.AppointmentSlotId, version }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentSlotResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AppointmentSlotResponse>> UpdateSlotAsync(Guid id, [FromBody] UpdateAppointmentSlotRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var updated = await _slotService.UpdateSlotAsync(id, request, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
