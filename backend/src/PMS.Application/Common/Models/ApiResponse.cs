namespace PMS.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<ApiError> Errors { get; init; } = Array.Empty<ApiError>();

    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? Array.Empty<ApiError>() };
}

public record ApiError(string? Field, string Message);
