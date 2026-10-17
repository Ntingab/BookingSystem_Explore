using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem_Explore.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int status = 500;
        string? detail = exception.Message;

        switch (exception)
        {
            case BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException:
                status = StatusCodes.Status401Unauthorized;
                break;
            case BookingSystem_Explore.BusinessLibrary.Exceptions.NotFoundException:
                status = StatusCodes.Status404NotFound;
                break;
            default:
                status = StatusCodes.Status500InternalServerError;
                break;
        }

        var pd = new ProblemDetails
        {
            Status = status,
            Title = GetReasonPhrase(status),
            Detail = detail
        };

        var json = JsonSerializer.Serialize(pd, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;
        return context.Response.WriteAsync(json);
    }

    private static string GetReasonPhrase(int status)
    {
        return status switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
    }
}
