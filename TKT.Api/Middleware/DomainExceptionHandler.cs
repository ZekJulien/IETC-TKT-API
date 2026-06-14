using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TKT.Api.Localization;
using TKT.Core.Domain.Exceptions;

namespace TKT.Api.Middleware;

public sealed class DomainExceptionHandler(IProblemDetailsService problemDetailsService, DomainErrorLocalizer localizer)
    : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly DomainErrorLocalizer _localizer = localizer;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException) return false;

        var statusCode = domainException switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            InvalidCredentialsException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Detail = _localizer[domainException.Code],
                Extensions = { ["code"] = domainException.Code },
            },
        });
    }
}
