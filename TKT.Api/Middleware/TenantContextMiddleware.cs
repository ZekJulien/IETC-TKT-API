using System.Security.Claims;
using TKT.Infrastructure.Persistence.Abstractions;
using TKT.Infrastructure.Security;

namespace TKT.Api.Middleware;

public sealed class TenantContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            if (Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
                tenantContext.AccountId = accountId;
            if (Guid.TryParse(user.FindFirstValue(AppClaims.CompanyId), out var companyId))
                tenantContext.CompanyId = companyId;
        }

        await _next(context);
    }
}
