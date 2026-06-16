namespace TKT.Core.IGateways;

public sealed record MemberDirectoryEntry(
    Guid AccountId,
    string Email,
    string Role,
    string? FirstName,
    string? LastName);
