using TKT.Core.Common;
using TKT.Core.Domain.Entities;

namespace TKT.Core.IGateways;

public interface ITicketsGateway
{
    Task<TicketCreated> CreateAsync(Ticket ticket);
    Task<int> CountCreatedThisMonthAsync(Guid companyId);
    Task<PagedResult<TicketSummary>> ListAsync(TicketListQuery query);
    Task<IReadOnlyList<StatusCount>> CountByStatusAsync(TicketListQuery query);
    Task<TicketStats> GetStatsAsync(Guid companyId, Guid currentUserId, bool restrictToOwn);
    Task<TicketDetail?> GetByIdAsync(Guid companyId, Guid ticketId);
    Task<TicketDetail?> UpdateAsync(TicketUpdate update);
}
