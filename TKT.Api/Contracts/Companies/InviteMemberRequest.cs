using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Companies;

public sealed class InviteMemberRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? JobTitle { get; set; }
}
