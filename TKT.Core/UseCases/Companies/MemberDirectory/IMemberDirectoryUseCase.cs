using TKT.Core.IGateways;

namespace TKT.Core.UseCases.Companies.MemberDirectory;

public interface IMemberDirectoryUseCase
{
    Task<IReadOnlyList<MemberDirectoryEntry>> ExecuteAsync(MemberDirectoryInput input);
}
