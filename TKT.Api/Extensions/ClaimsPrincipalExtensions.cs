using System.Security.Claims;
using TKT.Infrastructure.Security;

namespace TKT.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAccountId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    public static Guid? GetCompanyId(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue(AppClaims.CompanyId), out var id) ? id : null;
}
