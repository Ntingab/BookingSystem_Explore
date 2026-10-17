using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/appointment-slots")]
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
}
