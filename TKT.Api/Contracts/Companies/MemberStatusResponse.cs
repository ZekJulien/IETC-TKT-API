namespace TKT.Api.Contracts.Companies;

public sealed record MemberStatusResponse(Guid AccountId, bool IsActive);
