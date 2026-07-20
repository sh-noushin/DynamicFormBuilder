using FormBuilder.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.API.Middleware;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            DuplicateEntityException => (StatusCodes.Status400BadRequest, "Duplicate"),
            IdentityOperationFailedException => (StatusCodes.Status400BadRequest, "Identity Operation Failed"),
            InvalidValueException => (StatusCodes.Status400BadRequest, "Invalid Value"),
            InvalidCurrentPasswordException => (StatusCodes.Status400BadRequest, "Invalid Current Password"),
            FormSubmissionValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            FormClosedException => (StatusCodes.Status410Gone, "Form Closed"),
            DuplicateSubmissionException => (StatusCodes.Status409Conflict, "Duplicate Submission"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            _ => (0, string.Empty)
        };

        if (status == 0)
        {
            return false;
        }

        if (status >= 500)
        {
            _logger.LogError(exception, "Unhandled exception producing status {Status}", status);
        }
        else
        {
            _logger.LogInformation(
                exception,
                "Handled domain exception {ExceptionType} -> {Status}",
                exception.GetType().Name,
                status);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["message"] = exception.Message;

        if (exception is FormSubmissionValidationException validation)
        {
            problem.Extensions["fieldErrors"] = validation.FieldErrors;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
