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
}
