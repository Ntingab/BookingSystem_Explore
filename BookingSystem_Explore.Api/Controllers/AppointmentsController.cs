using BookingSystem_Explore.BusinessLibrary.DTOs.Requests;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.BusinessLibrary.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using BookingSystem_Explore.Infrastructure.Constants;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(Versions.Version1)]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

        // Extract management token from standard Authorization header (Bearer) or fallback to X-Management-Token
        private bool TryGetManagementToken(out string? token)
        {
            token = null;

            if (Request.Headers.TryGetValue("X-Management-Token", out var vals) && !string.IsNullOrEmpty(vals.FirstOrDefault()))
            {
                token = vals.First();
                return true;
            }

            if (Request.Headers.TryGetValue("Authorization", out var authVals))
            {
                var auth = authVals.FirstOrDefault();
                if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = auth.Substring("Bearer ".Length).Trim();
                    if (!string.IsNullOrEmpty(token)) return true;
                }
            }

            return false;
        }

    [HttpPost]
    public async Task<ActionResult<AppointmentCreatedResponse>> BookAppointmentAsync([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var created = await _appointmentService.BookAppointmentAsync(request, cancellationToken);
        // Use a named route for the GET by id so URL generation is robust
        var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1";
        return CreatedAtRoute("GetAppointmentById", new { id = created.AppointmentId, version }, created);
    }

    [HttpGet("{id:guid}", Name = "GetAppointmentById")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetManagementToken(out var token)) return Unauthorized();
        try
        {
            var res = await _appointmentService.GetAppointmentAsync(id, token!, cancellationToken);
            if (res == null) return NotFound();
            return Ok(res);
        }
        catch (BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpGet("reference/{reference}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (!Request.Headers.TryGetValue("X-Management-Token", out var vals) || string.IsNullOrEmpty(vals.FirstOrDefault()))
        {
            return Unauthorized();
        }
        var token = vals.First();
        try
        {
            var res = await _appointmentService.GetAppointmentByReferenceAsync(reference, token, cancellationToken);
            if (res == null) return NotFound();
            return Ok(res);
        }
        catch (BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentResponse>> CancelAppointmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetManagementToken(out var token)) return Unauthorized();
        try
        {
            var res = await _appointmentService.CancelAppointmentAsync(id, token!, cancellationToken);
            return Ok(res);
        }
        catch (BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException)
        {
            return Unauthorized();
        }
    }
}
