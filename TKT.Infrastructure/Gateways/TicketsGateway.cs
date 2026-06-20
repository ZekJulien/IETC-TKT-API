using TKT.Core.Common;
using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Infrastructure.Mappers;
using TKT.Infrastructure.Repositories.Abstractions;

namespace TKT.Infrastructure.Gateways;

public class TicketsGateway(ITicketsRepository repository) : ITicketsGateway
{
    private readonly ITicketsRepository _repository = repository;

    public async Task<TicketCreated> CreateAsync(Ticket ticket)
    {
        var created = await _repository.InsertAsync(ticket.ToRow());
        return created.ToCreated();
    }

    public Task<int> CountCreatedThisMonthAsync(Guid companyId)
        => _repository.CountCreatedThisMonthAsync(companyId);

    public async Task<PagedResult<TicketSummary>> ListAsync(TicketListQuery query)
    {
        var rows = await _repository.ListAsync(query);
        var total = await _repository.CountAsync(query);
        return new PagedResult<TicketSummary>(rows.Select(r => r.ToSummary()).ToList(), total, query.Page, query.PageSize);
    }

    public async Task<IReadOnlyList<StatusCount>> CountByStatusAsync(TicketListQuery query)
    {
        var rows = await _repository.CountByStatusAsync(query);
        return rows.Select(r => r.ToStatusCount()).ToList();
    }

    public async Task<TicketStats> GetStatsAsync(Guid companyId, Guid currentUserId, bool restrictToOwn)
    {
        var row = await _repository.GetStatsAsync(companyId, currentUserId, restrictToOwn);
        return row.ToStats();
    }

    public async Task<TicketDetail?> GetByIdAsync(Guid companyId, Guid ticketId)
    {
        var row = await _repository.GetByIdAsync(companyId, ticketId);
        return row?.ToDetail();
    }

    public async Task<TicketDetail?> UpdateAsync(TicketUpdate update)
    {
        var row = await _repository.UpdateAsync(update);
        return row?.ToDetail();
    }
}
