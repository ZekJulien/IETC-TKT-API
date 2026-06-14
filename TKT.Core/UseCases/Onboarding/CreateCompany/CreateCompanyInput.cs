namespace TKT.Core.UseCases.Onboarding.CreateCompany;

public sealed record CreateCompanyInput(Guid AccountId, string CompanyName, string CompanySlug);
