using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.GetTicketStats;

public interface IGetTicketStatsUseCase
{
    Task<TicketStats> ExecuteAsync(GetTicketStatsInput input);
}
