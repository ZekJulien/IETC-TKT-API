namespace TKT.Core.UseCases.Companies.ListMembers;

public sealed record ListMembersInput(
    Guid CompanyId,
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    int Page,
    int PageSize,
    string? Role,
    bool? IsActive);
