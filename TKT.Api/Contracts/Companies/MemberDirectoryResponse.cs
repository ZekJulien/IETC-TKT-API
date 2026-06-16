namespace TKT.Api.Contracts.Companies;

public sealed record MemberDirectoryResponse(
    Guid AccountId,
    string Email,
    string Role,
    string? FirstName,
    string? LastName);
