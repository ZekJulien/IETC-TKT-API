using TKT.Api.Contracts.Companies;
using TKT.Core.IGateways;
using TKT.Core.UseCases.Companies.ChangeMemberRole;
using TKT.Core.UseCases.Companies.InviteMember;
using TKT.Core.UseCases.Companies.ListMembers;
using TKT.Core.UseCases.Companies.SetMemberActive;

namespace TKT.Api.Mappers;

public static class CompanyMembersMapper
{
    public static InviteMemberInput ToInput(this InviteMemberRequest request, Guid companyId, Guid? callerCompanyId, Guid callerAccountId)
        => new(companyId, callerCompanyId, callerAccountId, request.Email, request.Role, request.Department, request.JobTitle);

    public static InviteMemberResponse ToResponse(this InviteMemberResult result)
        => new(result.Mode.ToString(), result.Role);

    public static ChangeMemberRoleInput ToInput(this ChangeMemberRoleRequest request, Guid companyId, Guid targetAccountId, Guid? callerCompanyId, Guid callerAccountId)
        => new(companyId, callerCompanyId, callerAccountId, targetAccountId, request.Role);

    public static SetMemberActiveInput ToInput(this SetMemberStatusRequest request, Guid companyId, Guid targetAccountId, Guid? callerCompanyId, Guid callerAccountId)
        => new(companyId, callerCompanyId, callerAccountId, targetAccountId, request.IsActive!.Value);

    public static ChangeMemberRoleResponse ToResponse(this ChangeMemberRoleResult result)
        => new(result.AccountId, result.Role);

    public static MemberStatusResponse ToResponse(this SetMemberActiveResult result)
        => new(result.AccountId, result.IsActive);

    public static MemberResponse ToResponse(this MemberSummary member)
        => new(member.AccountId, member.Email, member.DisplayName, member.Role, member.IsActive, member.JoinedAt);

    public static MemberListResponse ToResponse(this ListMembersResult result)
        => new(
            result.Members.Items.Select(m => m.ToResponse()).ToList(),
            result.Members.Total,
            result.Members.Page,
            result.Members.PageSize,
            result.ActiveMembers,
            result.MaxUsers);
}
