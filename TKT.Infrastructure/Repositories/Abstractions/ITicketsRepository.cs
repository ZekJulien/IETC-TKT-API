using TKT.Core.IGateways;
using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ITicketsRepository
{
    Task<TicketCreatedRow> InsertAsync(TicketRow ticket);
    Task<int> CountCreatedThisMonthAsync(Guid companyId);
    Task<IReadOnlyList<TicketSummaryRow>> ListAsync(TicketListQuery query);
    Task<int> CountAsync(TicketListQuery query);
    Task<IReadOnlyList<StatusCountRow>> CountByStatusAsync(TicketListQuery query);
    Task<TicketStatsRow> GetStatsAsync(Guid companyId, Guid currentUserId, bool restrictToOwn);
    Task<TicketDetailRow?> GetByIdAsync(Guid companyId, Guid ticketId);
    Task<TicketDetailRow?> UpdateAsync(TicketUpdate update);
}
