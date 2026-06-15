using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Identity.ListMyCompanies;

public sealed record ListMyCompaniesResult(IReadOnlyList<MemberCompany> Companies);
