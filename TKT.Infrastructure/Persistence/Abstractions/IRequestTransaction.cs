namespace TKT.Infrastructure.Persistence.Abstractions;

public interface IRequestTransaction
{
    Task CommitAsync();
    Task RollbackAsync();
}
