using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Mappers;

public static class CommentMapper
{
    public static CommentRow ToRow(this TicketComment comment)
        => new()
        {
            CommentId = comment.CommentId,
            CompanyId = comment.CompanyId,
            TicketId = comment.TicketId,
            AccountId = comment.AccountId,
            ReplyToId = comment.ReplyToId,
            CommentText = comment.CommentText,
            IsInternal = comment.IsInternal,
        };

    public static CommentSummary ToSummary(this CommentRow row)
        => new(row.CommentId, row.TicketId, row.AccountId, row.ReplyToId, row.CommentText,
               row.IsInternal, row.EditedAt, row.CreatedAt);
}
