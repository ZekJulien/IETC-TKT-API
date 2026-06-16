using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.MemberDirectory;

public sealed class MemberDirectoryUseCase(ICompanyMembersGateway members) : IMemberDirectoryUseCase
{
    private readonly ICompanyMembersGateway _members = members;

    public async Task<IReadOnlyList<MemberDirectoryEntry>> ExecuteAsync(MemberDirectoryInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (callerRole is null)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        return await _members.ListDirectoryAsync(input.CompanyId);
    }
}
