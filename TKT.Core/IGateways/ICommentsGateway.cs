using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ICommentsGateway
{
    Task<CommentSummary> CreateAsync(TicketComment comment);
    Task<IReadOnlyList<CommentSummary>> ListByTicketAsync(Guid companyId, Guid ticketId, bool includeInternal);
    Task<bool> ReplyTargetExistsAsync(Guid companyId, Guid ticketId, Guid replyToId, bool includeInternal);
    Task<CommentSummary?> GetAsync(Guid companyId, Guid commentId);
    Task<CommentSummary?> UpdateTextAsync(Guid companyId, Guid commentId, string text);
}
