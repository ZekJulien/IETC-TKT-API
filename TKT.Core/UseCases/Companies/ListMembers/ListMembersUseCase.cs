using TKT.Core.Common;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Companies.ListMembers;

public sealed class ListMembersUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICompanyMembersGateway members,
    ICompanySubscriptionGateway subscriptions,
    ICompanyInvitationGateway invitations) : IListMembersUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICompanySubscriptionGateway _subscriptions = subscriptions;
    private readonly ICompanyInvitationGateway _invitations = invitations;

    public async Task<ListMembersResult> ExecuteAsync(ListMembersInput input)
    {
        var callerRole = await _authorizer.ResolveForCompanyAsync(input.CallerCompanyId, input.CompanyId, input.CallerAccountId);
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
