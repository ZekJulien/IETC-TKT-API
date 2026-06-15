namespace TKT.Core.UseCases.Identity.ListMyCompanies;

public interface IListMyCompaniesUseCase
{
    Task<ListMyCompaniesResult> ExecuteAsync(ListMyCompaniesInput input);
}
