using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Comments;

public sealed class CreateCommentRequest
{
    [Required, MinLength(1), MaxLength(10000)]
    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; }

    public Guid? ReplyToId { get; set; }
}
