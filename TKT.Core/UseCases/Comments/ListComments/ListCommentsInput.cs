namespace TKT.Core.UseCases.Comments.ListComments;

public sealed record ListCommentsInput(
    Guid? CallerCompanyId,
    Guid CallerAccountId,
    Guid TicketId);
