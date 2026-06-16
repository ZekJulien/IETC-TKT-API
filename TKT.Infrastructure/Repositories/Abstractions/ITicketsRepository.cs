using TKT.Infrastructure.Models;

namespace TKT.Infrastructure.Repositories.Abstractions;

public interface ITicketsRepository
{
    Task<TicketCreatedRow> InsertAsync(TicketRow ticket);
    Task<int> CountCreatedThisMonthAsync(Guid companyId);
}
