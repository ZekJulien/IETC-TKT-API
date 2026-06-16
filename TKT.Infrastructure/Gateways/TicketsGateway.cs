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
}
