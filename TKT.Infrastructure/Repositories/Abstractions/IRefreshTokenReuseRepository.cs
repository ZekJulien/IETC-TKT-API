namespace TKT.Infrastructure.Repositories.Abstractions;

public interface IRefreshTokenReuseRepository
{
    Task RevokeFamilyAsync(Guid familyId, Guid accountId);
}
