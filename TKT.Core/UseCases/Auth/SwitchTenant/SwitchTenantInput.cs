namespace TKT.Core.UseCases.Auth.SwitchTenant;

public sealed record SwitchTenantInput(Guid AccountId, string Email, Guid CompanyId);
