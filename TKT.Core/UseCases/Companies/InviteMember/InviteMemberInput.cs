namespace TKT.Core.UseCases.Companies.InviteMember;

public sealed record InviteMemberInput(
    Guid CompanyId,
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    string Email,
    string Role,
    string? Department,
    string? JobTitle);
