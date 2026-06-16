namespace TKT.Core.UseCases.Tickets.ListTickets;

public interface IListTicketsUseCase
{
    Task<ListTicketsResult> ExecuteAsync(ListTicketsInput input);
}
