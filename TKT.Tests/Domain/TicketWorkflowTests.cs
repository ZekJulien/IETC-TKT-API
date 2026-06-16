using TKT.Core.Domain;
using Xunit;

namespace TKT.Tests.Domain;

public class TicketWorkflowTests
{
    [Theory]
    [InlineData(TicketStatuses.Open, TicketStatuses.InProgress)]
    [InlineData(TicketStatuses.InProgress, TicketStatuses.Pending)]
    [InlineData(TicketStatuses.InProgress, TicketStatuses.Resolved)]
    [InlineData(TicketStatuses.Pending, TicketStatuses.Resolved)]
    [InlineData(TicketStatuses.Resolved, TicketStatuses.Closed)]
    [InlineData(TicketStatuses.Resolved, TicketStatuses.InProgress)]
    [InlineData(TicketStatuses.Closed, TicketStatuses.InProgress)]
    public void CanTransition_AllowedMoves_IsTrue(string from, string to)
        => Assert.True(TicketWorkflow.CanTransition(from, to));

    [Theory]
    [InlineData(TicketStatuses.Open, TicketStatuses.Resolved)]
    [InlineData(TicketStatuses.Open, TicketStatuses.Closed)]
    [InlineData(TicketStatuses.Closed, TicketStatuses.Resolved)]
    [InlineData(TicketStatuses.Pending, TicketStatuses.Closed)]
    public void CanTransition_DisallowedMoves_IsFalse(string from, string to)
        => Assert.False(TicketWorkflow.CanTransition(from, to));

    [Fact]
    public void CanTransition_SameStatus_IsTrue()
        => Assert.True(TicketWorkflow.CanTransition(TicketStatuses.Open, TicketStatuses.Open));

    [Theory]
    [InlineData("open", true)]
    [InlineData("closed", true)]
    [InlineData("archived", false)]
    [InlineData("", false)]
    public void IsKnownStatus_ChecksAgainstWorkflow(string status, bool expected)
        => Assert.Equal(expected, TicketWorkflow.IsKnownStatus(status));
}
