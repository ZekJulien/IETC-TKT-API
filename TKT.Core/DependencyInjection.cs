using Microsoft.Extensions.DependencyInjection;
using TKT.Core.UseCases.Identity.GetMe;
using TKT.Core.UseCases.Auth.ConfirmEmail;
using TKT.Core.UseCases.Auth.Login;
using TKT.Core.UseCases.Auth.Register;
using TKT.Core.UseCases.Onboarding.CreateCompany;
using TKT.Core.UseCases.Onboarding.JoinInvitation;

namespace TKT.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<IRegisterAccountUseCase, RegisterAccountUseCase>();
        services.AddScoped<IConfirmEmailUseCase, ConfirmEmailUseCase>();
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<ICreateCompanyUseCase, CreateCompanyUseCase>();
        services.AddScoped<IJoinInvitationUseCase, JoinInvitationUseCase>();
        services.AddScoped<IGetMeUseCase, GetMeUseCase>();

        return services;
    }
}
