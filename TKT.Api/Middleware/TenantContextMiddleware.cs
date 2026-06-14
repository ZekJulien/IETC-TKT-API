using System.Security.Claims;
using TKT.Infrastructure.Abstractions;

namespace TKT.Api.Middleware;

public sealed class TenantContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true
            && Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
        {
            tenantContext.AccountId = accountId;
        }

        await _next(context);
    }
}
