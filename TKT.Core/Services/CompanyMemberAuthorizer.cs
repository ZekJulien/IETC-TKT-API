using TKT.Core.IGateways;

namespace TKT.Core.Services;

public sealed class CompanyMemberAuthorizer(ICompanyMembersGateway members) : ICompanyMemberAuthorizer
{
    private readonly ICompanyMembersGateway _members = members;

    public async Task<CallerMembership?> ResolveAsync(Guid? callerCompanyId, Guid callerAccountId)
    {
        if (callerCompanyId is not { } companyId)
            return null;

        var role = await _members.GetActiveRoleAsync(companyId, callerAccountId);
        return new CallerMembership(companyId, role);
    }

    public async Task<string?> ResolveForCompanyAsync(Guid? callerCompanyId, Guid targetCompanyId, Guid callerAccountId)
    {
        if (callerCompanyId != targetCompanyId)
            return null;

        return await _members.GetActiveRoleAsync(targetCompanyId, callerAccountId);
    }
}
