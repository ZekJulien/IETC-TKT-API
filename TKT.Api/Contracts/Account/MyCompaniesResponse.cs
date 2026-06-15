namespace TKT.Api.Contracts.Account;

public sealed record MyCompaniesResponse(IReadOnlyList<MyCompanyResponse> Companies);

public sealed record MyCompanyResponse(
    Guid CompanyId,
    string CompanyName,
    string CompanySlug,
    string? LogoUrl,
    string Role,
    bool IsActive);
