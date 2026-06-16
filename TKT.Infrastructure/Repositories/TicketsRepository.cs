using TKT.Core.IGateways;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Persistence;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class TicketsRepository(IDbSession db) : ITicketsRepository
{
    private readonly IDbSession _db = db;

    private const string FilterSql = """
                                     WHERE company_id = @CompanyId
                                       AND deleted_at IS NULL
                                       AND (@Status::text IS NULL OR status = @Status)
                                       AND (@Priority::text IS NULL OR priority = @Priority)
                                       AND (@AssignedTo::uuid IS NULL OR assigned_to = @AssignedTo)
                                       AND (@CategoryId::uuid IS NULL OR category_id = @CategoryId)
                                       AND (@RestrictToOwn = FALSE OR created_by = @CurrentUserId)
                                     """;

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

    public Task<IReadOnlyList<TicketSummaryRow>> ListAsync(TicketListQuery query)
    {
        var orderBy = query.Sort switch
        {
            "priority" => "CASE priority WHEN 'urgent' THEN 1 WHEN 'high' THEN 2 WHEN 'medium' THEN 3 WHEN 'low' THEN 4 ELSE 5 END, created_at DESC",
            "status" => "status, created_at DESC",
            _ => "created_at DESC",
        };

        var sql = $"""
                  SELECT ticket_id, ticket_number, title, status, priority,
                         created_by, assigned_to, category_id, created_at
                  FROM tickets
                  {FilterSql}
                  ORDER BY {orderBy}
                  LIMIT @PageSize OFFSET ((@Page - 1) * @PageSize);
                  """;
        return _db.QueryAsync<TicketSummaryRow>(sql, query);
    }

    public Task<int> CountAsync(TicketListQuery query)
    {
        var sql = $"SELECT COUNT(*)::int FROM tickets {FilterSql};";
        return _db.ExecuteScalarAsync<int>(sql, query);
    }

    public Task<IReadOnlyList<StatusCountRow>> CountByStatusAsync(TicketListQuery query)
    {
        const string sql = """
                           SELECT status, COUNT(*)::int AS count
                           FROM tickets
                           WHERE company_id = @CompanyId
                             AND deleted_at IS NULL
                             AND (@Priority::text IS NULL OR priority = @Priority)
                             AND (@AssignedTo::uuid IS NULL OR assigned_to = @AssignedTo)
                             AND (@CategoryId::uuid IS NULL OR category_id = @CategoryId)
                             AND (@RestrictToOwn = FALSE OR created_by = @CurrentUserId)
                           GROUP BY status;
                           """;
        return _db.QueryAsync<StatusCountRow>(sql, query);
    }
}
