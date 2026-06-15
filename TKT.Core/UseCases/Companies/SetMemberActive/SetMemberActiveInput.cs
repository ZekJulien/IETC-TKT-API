namespace TKT.Core.UseCases.Companies.SetMemberActive;

public sealed record SetMemberActiveInput(
    Guid CompanyId,
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid TargetAccountId,
    bool IsActive);
