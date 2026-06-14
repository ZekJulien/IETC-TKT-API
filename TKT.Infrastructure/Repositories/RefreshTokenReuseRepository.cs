using System.Data.Common;
using Dapper;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class RefreshTokenReuseRepository(IDbConnectionFactory factory) : IRefreshTokenReuseRepository
{
    private readonly IDbConnectionFactory _factory = factory;

    public async Task RevokeFamilyAsync(Guid familyId, Guid accountId)
    {
        await using var connection = (DbConnection)await _factory.CreateOpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(new CommandDefinition(
            "SELECT set_config('app.current_user_id', @AccountId, true)",
            new { AccountId = accountId.ToString() }, transaction));

        const string sql = """
                           UPDATE refresh_tokens
                           SET is_revoked = TRUE, revoked_at = NOW(), revoked_reason = 'reuse_detected'
                           WHERE family_id = @FamilyId AND is_revoked = FALSE;
                           """;
        await connection.ExecuteAsync(new CommandDefinition(sql, new { FamilyId = familyId }, transaction));

        await transaction.CommitAsync();
    }
}
