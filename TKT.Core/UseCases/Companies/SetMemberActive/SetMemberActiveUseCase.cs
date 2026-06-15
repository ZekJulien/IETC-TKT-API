using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.SetMemberActive;

public sealed class SetMemberActiveUseCase(
    ICompanyMembersGateway members,
    ICompanySubscriptionGateway subscriptions) : ISetMemberActiveUseCase
{
    private readonly ICompanyMembersGateway _members = members;
    private readonly ICompanySubscriptionGateway _subscriptions = subscriptions;

    public async Task<SetMemberActiveResult> ExecuteAsync(SetMemberActiveInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.SetMemberActive))
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var target = await _members.GetMemberAsync(input.CompanyId, input.TargetAccountId)
            ?? throw new NotFoundException(CompanyErrors.MemberNotFound);

        if (!input.IsActive && target.IsActive)
        {
            if (target.Role == CompanyRoles.Owner && await _members.CountActiveOwnersAsync(input.CompanyId) <= 1)
                throw new ConflictException(CompanyErrors.LastOwner);
        }
        else if (input.IsActive && !target.IsActive)
        {
            var maxUsers = await _subscriptions.GetMaxUsersAsync(input.CompanyId);
            if (await _members.CountActiveMembersAsync(input.CompanyId) >= maxUsers)
                throw new ConflictException(CompanyErrors.MemberQuotaExceeded);
        }

        await _members.SetActiveAsync(input.CompanyId, input.TargetAccountId, input.IsActive);
        return new SetMemberActiveResult(input.TargetAccountId, input.IsActive);
    }
}
