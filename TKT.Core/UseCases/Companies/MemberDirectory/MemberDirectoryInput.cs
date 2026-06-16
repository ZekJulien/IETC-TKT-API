namespace TKT.Core.UseCases.Companies.MemberDirectory;

public sealed record MemberDirectoryInput(
    Guid CompanyId,
    Guid? CallerCompanyId,
    Guid CallerAccountId);
