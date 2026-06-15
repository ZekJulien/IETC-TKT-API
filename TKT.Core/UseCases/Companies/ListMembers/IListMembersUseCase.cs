namespace TKT.Core.UseCases.Companies.ListMembers;

public interface IListMembersUseCase
{
    Task<ListMembersResult> ExecuteAsync(ListMembersInput input);
}
