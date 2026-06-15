using TKT.Core.Domain;

namespace TKT.Core.Domain.Authorization;

public static class CompanyAccessPolicy
{
    private static readonly Dictionary<string, HashSet<CompanyPermission>> Grants = new()
    {
        [CompanyRoles.Owner] = [CompanyPermission.InviteMember],
        [CompanyRoles.Admin] = [CompanyPermission.InviteMember],
    };

    public static bool Allows(string? role, CompanyPermission permission)
        => role is not null && Grants.TryGetValue(role, out var granted) && granted.Contains(permission);
}
