using TKT.Api.Extensions;
using TKT.Api.Localization;
using TKT.Api.Middleware;
using TKT.Api.Notifications;
using TKT.Core;
using TKT.Core.Abstractions;
using TKT.Infrastructure;
using Scalar.AspNetCore;
using TKT.Api.EndPoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCore();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IInvitationNotifier, InvitationNotifier>();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddCors();
builder.Services.AddLocalization();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<DomainErrorLocalizer>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

var app = builder.Build();

var supportedCultures = new[] { "en", "fr" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
    app.UseCors(c =>
    {
        c.AllowAnyHeader();
        c.AllowAnyMethod();
        c.AllowAnyOrigin();
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RequestContextMiddleware>();
app.UseMiddleware<TenantContextMiddleware>();
app.UseMiddleware<TransactionMiddleware>();

app.AddAuthRouter();
app.AddOnboardingRouter();
app.AddAccountRouter();
app.AddCompanyRouter();
app.AddTicketRouter();

app.Run();
