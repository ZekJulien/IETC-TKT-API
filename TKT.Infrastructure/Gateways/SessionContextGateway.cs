using TKT.Core.IGateways;
using TKT.Infrastructure.Persistence;

namespace TKT.Infrastructure.Gateways;

public class SessionContextGateway(IDbSession db) : ISessionContextGateway
{
    private readonly IDbSession _db = db;

    public Task SetCurrentUserAsync(Guid accountId)
        => _db.ExecuteAsync("SELECT set_config('app.current_user_id', @AccountId, true)",
            new { AccountId = accountId.ToString() });
}
