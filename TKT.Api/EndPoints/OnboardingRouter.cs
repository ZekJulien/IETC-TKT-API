using System.Security.Claims;
using TKT.Api.Contracts.Onboarding;
using TKT.Api.Extensions;
using TKT.Api.Mappers;
using TKT.Core.UseCases.Onboarding.CreateCompany;
using TKT.Core.UseCases.Onboarding.JoinInvitation;

namespace TKT.Api.EndPoints;

public static class OnboardingRouter
{
    public static WebApplication AddOnboardingRouter(this WebApplication app)
    {
        var group = app.MapGroup("api/onboarding").WithTags("onboarding");

        group.MapPost("create-company", async (CreateCompanyRequest req, ClaimsPrincipal user, ICreateCompanyUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(user.GetAccountId(), user.GetEmail()));
            return Results.Ok(result.ToResponse());
        });

        group.MapPost("join-invitation", async (JoinInvitationRequest req, ClaimsPrincipal user, IJoinInvitationUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(req.ToInput(user.GetAccountId(), user.GetEmail()));
            return Results.Ok(result.ToResponse());
        });

        return app;
    }
}
