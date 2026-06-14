namespace TKT.Api.Contracts.Onboarding;

public sealed record CreateCompanyResponse(Guid CompanyId, string CompanyName, string CompanySlug, string Role);
