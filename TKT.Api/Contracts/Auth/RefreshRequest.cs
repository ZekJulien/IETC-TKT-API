using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Auth;

public sealed class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
