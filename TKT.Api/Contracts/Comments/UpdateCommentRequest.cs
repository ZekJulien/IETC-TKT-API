using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Comments;

public sealed class UpdateCommentRequest
{
    [Required, MinLength(1), MaxLength(10000)]
    public string Content { get; set; } = string.Empty;
}
