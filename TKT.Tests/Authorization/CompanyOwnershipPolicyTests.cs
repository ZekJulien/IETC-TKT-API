using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using TKT.Core.Domain.Exceptions;
using Xunit;

namespace TKT.Tests.Authorization;

public class CompanyOwnershipPolicyTests
{
    [Fact]
    public void EnsureNotLastOwner_LastOwner_Throws()
        => Assert.Throws<ConflictException>(
            () => CompanyOwnershipPolicy.EnsureNotLastOwner(CompanyRoles.Owner, 1));

    [Fact]
    public void EnsureNotLastOwner_OwnerWithOtherOwners_DoesNotThrow()
        => CompanyOwnershipPolicy.EnsureNotLastOwner(CompanyRoles.Owner, 2);

    [Theory]
    [InlineData(CompanyRoles.Admin)]
    [InlineData(CompanyRoles.Agent)]
    [InlineData(CompanyRoles.Member)]
    public void EnsureNotLastOwner_NonOwner_DoesNotThrow(string role)
        => CompanyOwnershipPolicy.EnsureNotLastOwner(role, 1);
}
