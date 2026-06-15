using TKT.Api.Contracts.Companies;
using TKT.Core.UseCases.Companies.InviteMember;

namespace TKT.Api.Mappers;

public static class CompanyMembersMapper
{
    public static InviteMemberInput ToInput(this InviteMemberRequest request, Guid companyId, Guid? callerCompanyId, Guid callerAccountId)
        => new(companyId, callerCompanyId, callerAccountId, request.Email, request.Role, request.Department, request.JobTitle);

    public static InviteMemberResponse ToResponse(this InviteMemberResult result)
        => new(result.Mode.ToString(), result.Role);
}
