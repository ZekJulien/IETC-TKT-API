using TKT.Core.Domain;

namespace TKT.Core.Domain.Authorization;

public static class CompanyAccessPolicy
{
    private static readonly Dictionary<string, HashSet<CompanyPermission>> Grants = new()
    {
        [CompanyRoles.Owner] = [CompanyPermission.InviteMember, CompanyPermission.ListMembers, CompanyPermission.ChangeMemberRole, CompanyPermission.SetMemberActive],
        [CompanyRoles.Admin] = [CompanyPermission.InviteMember, CompanyPermission.ListMembers, CompanyPermission.ChangeMemberRole, CompanyPermission.SetMemberActive],
    };

    public static bool Allows(string? role, CompanyPermission permission)
        => role is not null && Grants.TryGetValue(role, out var granted) && granted.Contains(permission);
}
