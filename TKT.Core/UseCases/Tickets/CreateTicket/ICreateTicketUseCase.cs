namespace TKT.Core.UseCases.Tickets.CreateTicket;

public interface ICreateTicketUseCase
{
    Task<CreateTicketResult> ExecuteAsync(CreateTicketInput input);
}
