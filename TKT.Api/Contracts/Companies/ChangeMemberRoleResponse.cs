namespace TKT.Api.Contracts.Companies;

public sealed record ChangeMemberRoleResponse(Guid AccountId, string Role);
