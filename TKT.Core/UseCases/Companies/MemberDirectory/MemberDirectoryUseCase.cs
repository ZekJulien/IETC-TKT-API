using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Companies.MemberDirectory;

public sealed class MemberDirectoryUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICompanyMembersGateway members) : IMemberDirectoryUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICompanyMembersGateway _members = members;

    public async Task<IReadOnlyList<MemberDirectoryEntry>> ExecuteAsync(MemberDirectoryInput input)
    {
        var callerRole = await _authorizer.ResolveForCompanyAsync(input.CallerCompanyId, input.CompanyId, input.CallerAccountId);
        if (callerRole is null)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        return await _members.ListDirectoryAsync(input.CompanyId);
    }
}
