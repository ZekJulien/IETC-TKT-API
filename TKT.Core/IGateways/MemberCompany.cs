namespace TKT.Core.IGateways;

public sealed record MemberCompany(
    Guid CompanyId,
    string CompanyName,
    string CompanySlug,
    string? LogoUrl,
    string Role,
    bool IsActive);
