using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Errors;
using TKT.Core.Domain.Exceptions;
using TKT.Core.Domain.ValueObjects;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.ChangeMemberRole;

public sealed class ChangeMemberRoleUseCase(ICompanyMembersGateway members) : IChangeMemberRoleUseCase
{
    private readonly ICompanyMembersGateway _members = members;

    public async Task<ChangeMemberRoleResult> ExecuteAsync(ChangeMemberRoleInput input)
    {
        if (input.CallerCompanyId != input.CompanyId)
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var callerRole = await _members.GetActiveRoleAsync(input.CompanyId, input.CallerAccountId);
        if (!CompanyAccessPolicy.Allows(callerRole, CompanyPermission.ChangeMemberRole))
            throw new ForbiddenException(CompanyErrors.Forbidden);

        var newRole = MemberRole.CreateInvitable(input.NewRole);

        var target = await _members.GetMemberAsync(input.CompanyId, input.TargetAccountId)
            ?? throw new NotFoundException(CompanyErrors.MemberNotFound);

        if (target.Role == CompanyRoles.Owner && await _members.CountActiveOwnersAsync(input.CompanyId) <= 1)
            throw new ConflictException(CompanyErrors.LastOwner);

        await _members.UpdateRoleAsync(input.CompanyId, input.TargetAccountId, newRole.Value);
        return new ChangeMemberRoleResult(input.TargetAccountId, newRole.Value);
    }
}
