using System.Security.Claims;
using TKT.Api.Contracts.Companies;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Companies.InviteMember;

namespace TKT.Api.EndPoints;

public static class CompanyRouter
{
    public static WebApplication AddCompanyRouter(this WebApplication app)
    {
        var group = app.MapGroup("api/companies/{companyId:guid}/members").WithTags("companies");

        group.MapPost("invite", async (Guid companyId, InviteMemberRequest req, ClaimsPrincipal user, IInviteMemberUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(companyId, user.GetCompanyId(), user.GetAccountId()));
            return Results.Ok(result.ToResponse());
        });

        return app;
    }
}
