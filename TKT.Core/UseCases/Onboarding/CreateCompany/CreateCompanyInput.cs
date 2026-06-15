namespace TKT.Core.UseCases.Onboarding.CreateCompany;

public sealed record CreateCompanyInput(Guid AccountId, string Email, string CompanyName, string CompanySlug);
