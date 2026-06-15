using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Auth;

public sealed record SwitchTenantRequest([Required] Guid CompanyId);
