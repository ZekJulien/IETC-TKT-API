using TKT.Infrastructure.Abstractions;

namespace TKT.Api.Middleware;

public class RequestContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
    {
        requestContext.RequestAborted = context.RequestAborted;
        await _next(context);
    }
}