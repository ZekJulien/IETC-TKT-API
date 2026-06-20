using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;
using TKT.Core.Services;

namespace TKT.Core.UseCases.Companies.ChangeMemberRole;

public sealed class ChangeMemberRoleUseCase(
    ICompanyMemberAuthorizer authorizer,
    ICompanyMembersGateway members) : IChangeMemberRoleUseCase
{
    private readonly ICompanyMemberAuthorizer _authorizer = authorizer;
    private readonly ICompanyMembersGateway _members = members;

    public async Task<ChangeMemberRoleResult> ExecuteAsync(ChangeMemberRoleInput input)
    {
        var callerRole = await _authorizer.ResolveForCompanyAsync(input.CallerCompanyId, input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.ChangeMemberRole))
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var newRole = MemberRole.CreateInvitable(input.NewRole);

        var target = await _members.GetMemberAsync(input.CompanyId, input.TargetAccountId)
            ?? throw new NotFoundException(CompanyErrors.MemberNotFound);

        CompanyOwnershipPolicy.EnsureNotLastOwner(target.Role, await _members.CountActiveOwnersAsync(input.CompanyId));

        await _members.UpdateRoleAsync(input.CompanyId, input.TargetAccountId, newRole.Value);
        return new ChangeMemberRoleResult(input.TargetAccountId, newRole.Value);
    }
}
