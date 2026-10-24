using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        string? detail;

        switch (exception)
        {
            case DbUpdateException dbUpdateException when IsDuplicateBranchCode(dbUpdateException):
                status = StatusCodes.Status409Conflict;
                detail = "A branch with this branch code already exists.";
                break;
            case BookingSystem_Explore.BusinessLibrary.Exceptions.UnauthorizedAppointmentAccessException:
                status = StatusCodes.Status401Unauthorized;
                detail = exception.Message;
                break;
            case BookingSystem_Explore.BusinessLibrary.Exceptions.NotFoundException:
                status = StatusCodes.Status404NotFound;
                detail = exception.Message;
                break;
            default:
                status = StatusCodes.Status500InternalServerError;
                detail = "An unexpected error occurred.";
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

    private static bool IsDuplicateBranchCode(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("UQ_Branches_BranchCode", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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
