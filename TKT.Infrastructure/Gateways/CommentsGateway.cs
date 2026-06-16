using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class CommentsGateway(ICommentsRepository repository) : ICommentsGateway
{
    private readonly ICommentsRepository _repository = repository;

    public async Task<CommentSummary> CreateAsync(TicketComment comment)
    {
        var row = await _repository.InsertAsync(comment.ToRow());
        return row.ToSummary();
    }

    public async Task<IReadOnlyList<CommentSummary>> ListByTicketAsync(Guid companyId, Guid ticketId, bool includeInternal)
    {
        var rows = await _repository.ListByTicketAsync(companyId, ticketId, includeInternal);
        return rows.Select(r => r.ToSummary()).ToList();
    }

    public Task<bool> ReplyTargetExistsAsync(Guid companyId, Guid ticketId, Guid replyToId, bool includeInternal)
        => _repository.ReplyTargetExistsAsync(companyId, ticketId, replyToId, includeInternal);

    public async Task<CommentSummary?> GetAsync(Guid companyId, Guid commentId)
    {
        var row = await _repository.GetAsync(companyId, commentId);
        return row?.ToSummary();
    }

    public async Task<CommentSummary?> UpdateTextAsync(Guid companyId, Guid commentId, string text)
    {
        var row = await _repository.UpdateTextAsync(companyId, commentId, text);
        return row?.ToSummary();
    }
}
