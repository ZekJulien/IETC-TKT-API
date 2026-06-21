using TKT.Infrastructure.Persistence.Abstractions;

namespace TKT.Infrastructure.Persistence;

public sealed class RequestTransaction(DbSession session, SystemDbSession systemSession) : IRequestTransaction
{
    public async Task CommitAsync()
    {
        await session.CommitAsync();
        await systemSession.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await session.RollbackAsync();
        await systemSession.RollbackAsync();
    }
}
