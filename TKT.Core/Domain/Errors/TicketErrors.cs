namespace TKT.Core.Domain.Errors;

public static class TicketErrors
{
    public const string TitleInvalid = "ticket.title_invalid";
    public const string TitleTooLong = "ticket.title_too_long";
    public const string PriorityInvalid = "ticket.priority_invalid";
    public const string SourceInvalid = "ticket.source_invalid";
    public const string QuotaExceeded = "ticket.quota_exceeded";
    public const string AssigneeInvalid = "ticket.assignee_invalid";
    public const string SortInvalid = "ticket.sort_invalid";
    public const string NotFound = "ticket.not_found";
    public const string StatusInvalid = "ticket.status_invalid";
    public const string InvalidTransition = "ticket.invalid_transition";
    public const string Forbidden = "ticket.forbidden";
}
