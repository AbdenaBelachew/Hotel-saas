namespace PMS.Application.Common.Exceptions;

/// <summary>Base type for exceptions the global exception middleware translates into an ApiResponse.</summary>
public abstract class AppException(string message) : Exception(message);

public sealed class NotFoundException(string entity, object key)
    : AppException($"{entity} with id '{key}' was not found.");

public sealed class ConflictException(string message) : AppException(message);

public sealed class UnauthorizedAppException(string message = "Invalid credentials.") : AppException(message);

public sealed class ForbiddenException(string message = "You do not have permission to perform this action.")
    : AppException(message);

public sealed class ValidationAppException(IReadOnlyList<(string Field, string Message)> errors)
    : AppException("One or more validation errors occurred.")
{
    public IReadOnlyList<(string Field, string Message)> Errors { get; } = errors;
}
