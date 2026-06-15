namespace TKT.Core.UseCases.Companies.ChangeMemberRole;

public sealed record ChangeMemberRoleInput(
    Guid CompanyId,
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid TargetAccountId,
    string NewRole);
