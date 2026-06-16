using System.Security.Claims;
using TKT.Api.Contracts.Tickets;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Tickets.CreateTicket;

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

        return app;
    }
}
