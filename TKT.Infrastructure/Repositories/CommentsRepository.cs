using TKT.Infrastructure.Persistence.Abstractions;
using TKT.Infrastructure.Models;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Repositories;

public class CommentsRepository(IDbSession db) : ICommentsRepository
{
    private readonly IDbSession _db = db;

    private const string SelectColumns =
        "comment_id, ticket_id, account_id, reply_to_id, comment_text, is_internal, edited_at, created_at";

    public async Task<CommentRow> InsertAsync(CommentRow comment)
    {
        var sql = $"""
                  INSERT INTO ticket_comments (comment_id, company_id, ticket_id, account_id,
                                               reply_to_id, comment_text, is_internal)
                  VALUES (@CommentId, @CompanyId, @TicketId, @AccountId,
                          @ReplyToId, @CommentText, @IsInternal)
                  RETURNING {SelectColumns};
                  """;
        var row = await _db.QuerySingleOrDefaultAsync<CommentRow>(sql, comment);
        return row!;
    }

    public Task<IReadOnlyList<CommentRow>> ListByTicketAsync(Guid companyId, Guid ticketId, bool includeInternal)
    {
        var sql = $"""
                  SELECT {SelectColumns}
                  FROM ticket_comments
                  WHERE ticket_id = @TicketId AND company_id = @CompanyId
                    AND (@IncludeInternal OR is_internal = FALSE)
                  ORDER BY created_at;
                  """;
        return _db.QueryAsync<CommentRow>(sql, new { CompanyId = companyId, TicketId = ticketId, IncludeInternal = includeInternal });
    }

    public Task<bool> ReplyTargetExistsAsync(Guid companyId, Guid ticketId, Guid replyToId, bool includeInternal)
    {
        const string sql = """
                           SELECT EXISTS(
                               SELECT 1 FROM ticket_comments
                               WHERE comment_id = @ReplyToId AND ticket_id = @TicketId AND company_id = @CompanyId
                                 AND (@IncludeInternal OR is_internal = FALSE)
                           );
                           """;
        return _db.ExecuteScalarAsync<bool>(sql, new { CompanyId = companyId, TicketId = ticketId, ReplyToId = replyToId, IncludeInternal = includeInternal });
    }

    public Task<CommentRow?> GetAsync(Guid companyId, Guid commentId)
    {
        var sql = $"""
                  SELECT {SelectColumns}
                  FROM ticket_comments
                  WHERE comment_id = @CommentId AND company_id = @CompanyId;
                  """;
        return _db.QuerySingleOrDefaultAsync<CommentRow>(sql, new { CompanyId = companyId, CommentId = commentId });
    }

    public Task<CommentRow?> UpdateTextAsync(Guid companyId, Guid commentId, string text)
    {
        var sql = $"""
                  UPDATE ticket_comments
                  SET comment_text = @Text, edited_at = NOW()
                  WHERE comment_id = @CommentId AND company_id = @CompanyId
                  RETURNING {SelectColumns};
                  """;
        return _db.QuerySingleOrDefaultAsync<CommentRow>(sql, new { CompanyId = companyId, CommentId = commentId, Text = text });
    }
}
