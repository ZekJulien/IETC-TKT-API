using TKT.Core.Common;
using TKT.Core.Domain.Entities;
using TKT.Core.IGateways;
using TKT.Core.Services;
using Xunit;

namespace TKT.Tests.Authorization;

public class CompanyMemberAuthorizerTests
{
    private static readonly Guid Company = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid Account = Guid.Parse("22222222-2222-4222-8222-222222222222");

    [Fact]
    public async Task ResolveAsync_NoTenantContext_ReturnsNull()
    {
        var authorizer = new CompanyMemberAuthorizer(new FakeMembers("agent"));
        var result = await authorizer.ResolveAsync(null, Account);
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_WithTenant_ReturnsCompanyAndRole()
    {
        var authorizer = new CompanyMemberAuthorizer(new FakeMembers("admin"));
        var result = await authorizer.ResolveAsync(Company, Account);
        Assert.NotNull(result);
        Assert.Equal(Company, result!.CompanyId);
        Assert.Equal("admin", result.Role);
    }

    [Fact]
    public async Task ResolveForCompanyAsync_DifferentCompany_ReturnsNull()
    {
        var authorizer = new CompanyMemberAuthorizer(new FakeMembers("owner"));
        var other = Guid.Parse("33333333-3333-4333-8333-333333333333");
        var role = await authorizer.ResolveForCompanyAsync(other, Company, Account);
        Assert.Null(role);
    }

    [Fact]
    public async Task ResolveForCompanyAsync_SameCompany_ReturnsRole()
    {
        var authorizer = new CompanyMemberAuthorizer(new FakeMembers("owner"));
        var role = await authorizer.ResolveForCompanyAsync(Company, Company, Account);
        Assert.Equal("owner", role);
    }

    private sealed class FakeMembers(string? role) : ICompanyMembersGateway
    {
        public Task<string?> GetActiveRoleAsync(Guid companyId, Guid accountId) => Task.FromResult(role);

        public Task<bool> MemberExistsAsync(Guid companyId, Guid accountId) => throw new NotSupportedException();
        public Task AddMemberAsync(CompanyMember member) => throw new NotSupportedException();
        public Task<int> CountActiveMembersAsync(Guid companyId) => throw new NotSupportedException();
        public Task<CompanyMember?> GetMemberAsync(Guid companyId, Guid accountId) => throw new NotSupportedException();
        public Task<int> CountActiveOwnersAsync(Guid companyId) => throw new NotSupportedException();
        public Task UpdateRoleAsync(Guid companyId, Guid accountId, string role) => throw new NotSupportedException();
        public Task SetActiveAsync(Guid companyId, Guid accountId, bool isActive) => throw new NotSupportedException();
        public Task<PagedResult<MemberSummary>> ListAsync(Guid companyId, int page, int pageSize, string? role, bool? isActive) => throw new NotSupportedException();
        public Task<IReadOnlyList<MemberDirectoryEntry>> ListDirectoryAsync(Guid companyId) => throw new NotSupportedException();
    }
}
