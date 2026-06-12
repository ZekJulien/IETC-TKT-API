using Microsoft.Extensions.DependencyInjection;
using TKT.Core.UseCases.Auth.Register;

namespace TKT.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<IRegisterAccountUseCase, RegisterAccountUseCase>();

        return services;
    }
}
