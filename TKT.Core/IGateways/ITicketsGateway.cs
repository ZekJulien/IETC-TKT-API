using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ITicketsGateway
{
    Task<TicketCreated> CreateAsync(Ticket ticket);
    Task<int> CountCreatedThisMonthAsync(Guid companyId);
}
