using System.Net;
using System.Text.Json;
using FluentValidation;
using PMS.Application.Common.Exceptions;
using PMS.Application.Common.Models;

namespace PMS.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(
                    "One or more validation errors occurred.",
                    validationException.Errors.Select(e => new ApiError(e.PropertyName, e.ErrorMessage)).ToList())),

            ValidationAppException validationAppException => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(
                    validationAppException.Message,
                    validationAppException.Errors.Select(e => new ApiError(e.Field, e.Message)).ToList())),

            NotFoundException notFound => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(notFound.Message)),
            ConflictException conflict => (HttpStatusCode.Conflict, ApiResponse<object>.Fail(conflict.Message)),
            ForbiddenException forbidden => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(forbidden.Message)),
            UnauthorizedAppException unauthorized => (HttpStatusCode.Unauthorized, ApiResponse<object>.Fail(unauthorized.Message)),

            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred. Please try again later."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("{ExceptionType} handling {Method} {Path}: {Message}",
                exception.GetType().Name, context.Request.Method, context.Request.Path, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
