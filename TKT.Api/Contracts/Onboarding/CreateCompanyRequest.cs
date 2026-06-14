using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Onboarding;

public sealed class CreateCompanyRequest
{
    [Required, MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string CompanySlug { get; set; } = string.Empty;
}
