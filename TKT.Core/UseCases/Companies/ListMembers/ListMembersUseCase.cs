using TKT.Core.Common;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.ListMembers;

public sealed class ListMembersUseCase(
    ICompanyMembersGateway members,
    ICompanySubscriptionGateway subscriptions,
    ICompanyInvitationGateway invitations) : IListMembersUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICompanySubscriptionGateway _subscriptions = subscriptions;
    private readonly ICompanyInvitationGateway _invitations = invitations;

    public async Task<ListMembersResult> ExecuteAsync(ListMembersInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.ListMembers))
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var pagination = Pagination.Create(input.Page, input.PageSize);
        var page = await _members.ListAsync(input.CompanyId, pagination.Page, pagination.PageSize, input.Role, input.IsActive);
        var activeMembers = await _members.CountActiveMembersAsync(input.CompanyId);
        var pendingInvitations = await _invitations.CountActivePendingAsync(input.CompanyId);
        var maxUsers = await _subscriptions.GetMaxUsersAsync(input.CompanyId);

        return new ListMembersResult(page, activeMembers, pendingInvitations, maxUsers);
    }
}
