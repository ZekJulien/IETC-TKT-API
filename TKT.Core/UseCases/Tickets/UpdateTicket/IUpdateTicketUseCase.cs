using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Tickets.UpdateTicket;

public interface IUpdateTicketUseCase
{
    Task<TicketDetail> ExecuteAsync(UpdateTicketInput input);
}
