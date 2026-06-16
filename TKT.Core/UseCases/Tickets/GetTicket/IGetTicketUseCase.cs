using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.GetTicket;

public interface IGetTicketUseCase
{
    Task<TicketDetail> ExecuteAsync(GetTicketInput input);
}
