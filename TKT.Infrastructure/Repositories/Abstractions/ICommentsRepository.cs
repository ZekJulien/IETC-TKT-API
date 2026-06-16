using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ICommentsRepository
{
    Task<CommentRow> InsertAsync(CommentRow comment);
    Task<IReadOnlyList<CommentRow>> ListByTicketAsync(Guid companyId, Guid ticketId, bool includeInternal);
    Task<bool> ReplyTargetExistsAsync(Guid companyId, Guid ticketId, Guid replyToId, bool includeInternal);
    Task<CommentRow?> GetAsync(Guid companyId, Guid commentId);
    Task<CommentRow?> UpdateTextAsync(Guid companyId, Guid commentId, string text);
}
