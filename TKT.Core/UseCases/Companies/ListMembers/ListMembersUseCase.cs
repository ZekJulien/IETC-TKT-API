using TKT.Core.Common;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.ListMembers;

public sealed class ListMembersUseCase(ICompanyMembersGateway members) : IListMembersUseCase
{
    private readonly ICompanyMembersGateway _members = members;

    public async Task<PagedResult<MemberSummary>> ExecuteAsync(ListMembersInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.ListMembers))
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var pagination = Pagination.Create(input.Page, input.PageSize);
        return await _members.ListAsync(input.CompanyId, pagination.Page, pagination.PageSize, input.Role, input.IsActive);
    }
}
