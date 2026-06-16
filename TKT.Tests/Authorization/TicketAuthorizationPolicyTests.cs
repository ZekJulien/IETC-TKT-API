using TKT.Core.Domain;
using TKT.Core.Domain.Authorization;
using Xunit;

namespace TKT.Tests.Authorization;

public class TicketAuthorizationPolicyTests
{
    [Theory]
    [InlineData(CompanyRoles.Owner)]
    [InlineData(CompanyRoles.Admin)]
    [InlineData(CompanyRoles.Agent)]
    [InlineData(CompanyRoles.Member)]
    public void CanCreate_WithActiveRole_IsTrue(string role)
        => Assert.True(TicketAuthorizationPolicy.CanCreate(role));

    [Fact]
    public void CanCreate_WithNullRole_IsFalse()
        => Assert.False(TicketAuthorizationPolicy.CanCreate(null));

    [Theory]
    [InlineData(CompanyRoles.Owner)]
    [InlineData(CompanyRoles.Member)]
    public void CanList_WithActiveRole_IsTrue(string role)
        => Assert.True(TicketAuthorizationPolicy.CanList(role));

    [Fact]
    public void CanList_WithNullRole_IsFalse()
        => Assert.False(TicketAuthorizationPolicy.CanList(null));

    [Fact]
    public void RestrictsToOwnTickets_OnlyForMember()
    {
        Assert.True(TicketAuthorizationPolicy.RestrictsToOwnTickets(CompanyRoles.Member));
        Assert.False(TicketAuthorizationPolicy.RestrictsToOwnTickets(CompanyRoles.Owner));
        Assert.False(TicketAuthorizationPolicy.RestrictsToOwnTickets(CompanyRoles.Admin));
        Assert.False(TicketAuthorizationPolicy.RestrictsToOwnTickets(CompanyRoles.Agent));
        Assert.False(TicketAuthorizationPolicy.RestrictsToOwnTickets(null));
    }
}
