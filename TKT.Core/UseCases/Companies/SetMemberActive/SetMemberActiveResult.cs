namespace TKT.Core.UseCases.Companies.SetMemberActive;

public sealed record SetMemberActiveResult(Guid AccountId, bool IsActive);
