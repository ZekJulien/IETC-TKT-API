using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class TicketsRepository(IDbSession db) : ITicketsRepository
{
    private readonly IDbSession _db = db;

    public async Task<TicketCreatedRow> InsertAsync(TicketRow ticket)
    {
        const string sql = """
                           INSERT INTO tickets (ticket_id, company_id, title, description, status,
                                                priority, created_by, assigned_to, category_id, source)
                           VALUES (@TicketId, @CompanyId, @Title, @Description, @Status,
                                   @Priority, @CreatedBy, @AssignedTo, @CategoryId, @Source)
                           RETURNING ticket_number, created_at;
                           """;
        var row = await _db.QuerySingleOrDefaultAsync<TicketCreatedRow>(sql, ticket);
        return row!;
    }

    public Task<int> CountCreatedThisMonthAsync(Guid companyId)
    {
        const string sql = """
                           SELECT COUNT(*)::int
                           FROM tickets
                           WHERE company_id = @CompanyId
                             AND created_at >= date_trunc('month', NOW());
                           """;
        return _db.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }
}
