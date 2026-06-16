using TKT.Core.Domain;

namespace TKT.Core.Domain.Authorization;

public static class TicketAuthorizationPolicy
{
    private static readonly HashSet<string> CanCreateRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        CompanyRoles.Owner,
        CompanyRoles.Admin,
        CompanyRoles.Agent,
        CompanyRoles.Member,
    };

    private static readonly HashSet<string> CanModifyRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        CompanyRoles.Owner,
        CompanyRoles.Admin,
        CompanyRoles.Agent,
    };

    public static bool CanCreate(string? role)
        => role is not null && CanCreateRoles.Contains(role);

    public static bool CanModify(string? role)
        => role is not null && CanModifyRoles.Contains(role);

    public static bool CanList(string? role)
        => role is not null;

    public static bool RestrictsToOwnTickets(string? role)
        => string.Equals(role, CompanyRoles.Member, StringComparison.OrdinalIgnoreCase);
}
