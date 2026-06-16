using TKT.Core.Common;
using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ITicketsGateway
{
    Task<TicketCreated> CreateAsync(Ticket ticket);
    Task<int> CountCreatedThisMonthAsync(Guid companyId);
    Task<PagedResult<TicketSummary>> ListAsync(TicketListQuery query);
    Task<IReadOnlyList<StatusCount>> CountByStatusAsync(TicketListQuery query);
    Task<TicketDetail?> GetByIdAsync(Guid companyId, Guid ticketId);
    Task<TicketDetail?> UpdateAsync(TicketUpdate update);
}
