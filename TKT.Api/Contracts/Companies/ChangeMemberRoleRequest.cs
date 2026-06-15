using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Companies;

public sealed class ChangeMemberRoleRequest
{
    [Required, MaxLength(20)]
    public string Role { get; set; } = string.Empty;
}
