using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Comments.CreateComment;

public interface ICreateCommentUseCase
{
    Task<CommentSummary> ExecuteAsync(CreateCommentInput input);
}
