using System.Security.Claims;
using TKT.Api.Contracts.Tickets;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Tickets.CreateTicket;
using TKT.Core.UseCases.Tickets.ListTickets;
using TKT.Core.UseCases.Tickets.GetTicket;
using TKT.Core.UseCases.Tickets.UpdateTicket;

namespace TKT.Api.EndPoints;

public static class TicketRouter
{
    public static WebApplication AddTicketRouter(this WebApplication app)
    {
        var group = app.MapGroup("api/tickets").WithTags("tickets");

        group.MapPost("", async (CreateTicketRequest req, ClaimsPrincipal user, ICreateTicketUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(user.GetCompanyId(), user.GetAccountId()));
            return Results.Created($"/api/tickets/{result.TicketId}", result.ToResponse());
        });

        group.MapGet("", async (ClaimsPrincipal user, IListTicketsUseCase useCase,
            string? status, string? priority, Guid? assignedTo, Guid? categoryId,
            string? sort, int? page, int? pageSize) =>
        {
            var input = new ListTicketsInput(
                user.GetCompanyId(), user.GetAccountId(),
                status, priority, assignedTo, categoryId, sort,
                page ?? 1, pageSize ?? 20);
            var result = await useCase.ExecuteAsync(input);
            return Results.Ok(result.ToResponse());
        });

        group.MapGet("{ticketId:guid}", async (Guid ticketId, ClaimsPrincipal user, IGetTicketUseCase useCase) =>
        {
            var detail = await useCase.ExecuteAsync(new GetTicketInput(user.GetCompanyId(), user.GetAccountId(), ticketId));
            return Results.Ok(detail.ToResponse());
        });

        group.MapPatch("{ticketId:guid}", async (Guid ticketId, UpdateTicketRequest req, ClaimsPrincipal user, IUpdateTicketUseCase useCase) =>
        {
            var detail = await useCase.ExecuteAsync(req.ToInput(user.GetCompanyId(), user.GetAccountId(), ticketId));
            return Results.Ok(detail.ToResponse());
        });

        return app;
    }
}
