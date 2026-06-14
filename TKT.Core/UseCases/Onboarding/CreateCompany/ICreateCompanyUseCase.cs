namespace TKT.Core.UseCases.Onboarding.CreateCompany;

public interface ICreateCompanyUseCase
{
    Task<CreateCompanyResult> ExecuteAsync(CreateCompanyInput input);
}
