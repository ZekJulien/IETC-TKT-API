using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Comments.ListComments;

public interface IListCommentsUseCase
{
    Task<IReadOnlyList<CommentSummary>> ExecuteAsync(ListCommentsInput input);
}
