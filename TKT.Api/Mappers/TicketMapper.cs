using TKT.Api.Contracts.Tickets;
using TKT.Core.UseCases.Tickets.CreateTicket;

namespace TKT.Api.Mappers;

public static class TicketMapper
{
    public static CreateTicketInput ToInput(this CreateTicketRequest request, Guid? callerCompanyId, Guid callerAccountId)
        => new(callerCompanyId, callerAccountId, request.Title, request.Description, request.Priority, request.CategoryId, request.AssignedTo, request.Source);

    public static CreateTicketResponse ToResponse(this CreateTicketResult result)
        => new(result.TicketId, result.TicketNumber, result.Status, result.Priority, result.CreatedAt);
}
