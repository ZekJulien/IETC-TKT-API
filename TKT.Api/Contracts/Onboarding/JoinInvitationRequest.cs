using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Onboarding;

public sealed class JoinInvitationRequest
{
    [Required, MaxLength(64)]
    public string InvitationCode { get; set; } = string.Empty;
}
