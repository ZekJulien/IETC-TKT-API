namespace TKT.Core.UseCases.Onboarding.CreateCompany;

public sealed record CreateCompanyResult(Guid CompanyId, string CompanyName, string CompanySlug, string Role);
