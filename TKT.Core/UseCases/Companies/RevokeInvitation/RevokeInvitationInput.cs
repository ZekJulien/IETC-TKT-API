namespace TKT.Core.UseCases.Companies.RevokeInvitation;

public sealed record RevokeInvitationInput(
    Guid CompanyId,
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid InvitationId);
