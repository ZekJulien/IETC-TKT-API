namespace TKT.Core.Abstractions;

public sealed record EmailConfirmationToken(Guid AccountId, string SecurityStamp);
