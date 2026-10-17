using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingSystem_Explore.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateManagementTokenAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // enforce HTTPS except in Development (allow integration tests / dev)
        var env = context.HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Hosting.IHostEnvironment)) as Microsoft.Extensions.Hosting.IHostEnvironment;
        if (!(env?.IsDevelopment() == true) && !context.HttpContext.Request.IsHttps)
        {
            context.Result = new ForbidResult();
            return;
        }

        // header required
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Management-Token", out var values) || string.IsNullOrEmpty(values.FirstOrDefault()))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var token = values.First();

        // try to resolve appointment identifier from route values
        var route = context.RouteData.Values;
        var services = context.HttpContext.RequestServices;
        var tokenService = services.GetService(typeof(BookingSystem_Explore.Domain.Security.ITokenService)) as BookingSystem_Explore.Domain.Security.ITokenService;
        var appointmentService = services.GetService(typeof(BookingSystem_Explore.BusinessLibrary.Interfaces.Services.IAppointmentService)) as BookingSystem_Explore.BusinessLibrary.Interfaces.Services.IAppointmentService;

        if (appointmentService == null || tokenService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        BookingSystem_Explore.BusinessLibrary.DTOs.Responses.AppointmentResponse? apptResp = null;

        try
        {
            if (route.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var id))
            {
                // Use business service to validate token (it will throw if unauthorized)
                apptResp = await appointmentService.GetAppointmentAsync(id, token, context.HttpContext.RequestAborted);
            }
            else if (route.TryGetValue("appointmentId", out var aidObj) && Guid.TryParse(aidObj?.ToString(), out var aid))
            {
                apptResp = await appointmentService.GetAppointmentAsync(aid, token, context.HttpContext.RequestAborted);
            }
            else if (route.TryGetValue("reference", out var referenceObj) && referenceObj != null)
            {
                var reference = referenceObj.ToString() ?? string.Empty;
                apptResp = await appointmentService.GetAppointmentByReferenceAsync(reference, token, context.HttpContext.RequestAborted);
            }
            else
            {
                // No appointment identifier found; cannot validate
                context.Result = new BadRequestObjectResult(new { error = "Missing appointment identifier in route" });
                return;
            }
        }
        catch (BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        catch (BookingSystem_Explore.BusinessLibrary.Exceptions.NotFoundException)
        {
            context.Result = new NotFoundResult();
            return;
        }

        if (apptResp == null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        // token valid, proceed
        await next();
    }
}
