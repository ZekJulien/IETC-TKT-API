using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Companies;

public sealed class SetMemberStatusRequest
{
    [Required]
    public bool? IsActive { get; set; }
}
