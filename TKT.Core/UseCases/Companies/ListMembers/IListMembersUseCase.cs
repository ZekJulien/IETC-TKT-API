using TKT.Core.Common;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.ListMembers;

public interface IListMembersUseCase
{
    Task<PagedResult<MemberSummary>> ExecuteAsync(ListMembersInput input);
}
