namespace TKT.Core.Domain.Errors;

public static class CommentErrors
{
    public const string NotFound = "comment.not_found";
    public const string TextInvalid = "comment.text_invalid";
    public const string ReplyInvalid = "comment.reply_invalid";
    public const string Forbidden = "comment.forbidden";
    public const string TicketUnassigned = "comment.ticket_unassigned";
}
