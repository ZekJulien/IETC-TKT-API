using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Comments.UpdateComment;

public interface IUpdateCommentUseCase
{
    Task<CommentSummary> ExecuteAsync(UpdateCommentInput input);
}
