using TKT.Core.Common;
using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.ListMembers;

public sealed record ListMembersResult(
    PagedResult<MemberSummary> Members,
    int ActiveMembers,
    int MaxUsers);
