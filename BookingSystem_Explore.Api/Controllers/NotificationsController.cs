using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using BookingSystem_Explore.BusinessLibrary.Interfaces.Services;
using BookingSystem_Explore.Infrastructure.Constants;

namespace BookingSystem_Explore.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/appointments/{appointmentId:guid}/notifications")]
[ApiVersion(Versions.Version1)]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("/api/v{version:apiVersion}/notifications")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    public async Task<ActionResult<IEnumerable<object>>> GetAllNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _notificationService.GetAllNotificationsAsync(cancellationToken);
        return Ok(list);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<object>>> GetAppointmentNotificationsAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        // management token must be provided via header only
        if (!Request.Headers.TryGetValue("X-Management-Token", out var values) || string.IsNullOrEmpty(values.FirstOrDefault()))
        {
            return Unauthorized();
        }

        var token = values.First();

        try
        {
            var list = await _notificationService.GetAppointmentNotificationsAsync(appointmentId, token, cancellationToken);
            if (list == null || list.Count == 0) return NotFound();
            return Ok(list);
        }
        catch (BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException)
        {
            return Unauthorized();
        }
    }
}
