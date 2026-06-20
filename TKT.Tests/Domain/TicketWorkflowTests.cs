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

    [Fact]
    public void NextStatusOnComment_StaffPublicOnInProgress_GoesPending()
        => Assert.Equal(TicketStatuses.Pending,
            TicketWorkflow.NextStatusOnComment(TicketStatuses.InProgress, authorIsStaff: true, isInternal: false));

    [Fact]
    public void NextStatusOnComment_RequesterPublicOnPending_GoesInProgress()
        => Assert.Equal(TicketStatuses.InProgress,
            TicketWorkflow.NextStatusOnComment(TicketStatuses.Pending, authorIsStaff: false, isInternal: false));

    [Theory]
    [InlineData(TicketStatuses.InProgress, true)]
    [InlineData(TicketStatuses.Pending, false)]
    public void NextStatusOnComment_InternalComment_NeverChangesStatus(string current, bool authorIsStaff)
        => Assert.Null(TicketWorkflow.NextStatusOnComment(current, authorIsStaff, isInternal: true));

    [Theory]
    [InlineData(TicketStatuses.InProgress, false)]
    [InlineData(TicketStatuses.Pending, true)]
    [InlineData(TicketStatuses.Open, true)]
    [InlineData(TicketStatuses.Open, false)]
    [InlineData(TicketStatuses.Resolved, true)]
    [InlineData(TicketStatuses.Closed, false)]
    public void NextStatusOnComment_NoPingPongCase_ReturnsNull(string current, bool authorIsStaff)
        => Assert.Null(TicketWorkflow.NextStatusOnComment(current, authorIsStaff, isInternal: false));

    [Fact]
    public void NextStatusOnAssignment_OpenTicket_GoesInProgress()
        => Assert.Equal(TicketStatuses.InProgress, TicketWorkflow.NextStatusOnAssignment(TicketStatuses.Open));

    [Theory]
    [InlineData(TicketStatuses.InProgress)]
    [InlineData(TicketStatuses.Pending)]
    [InlineData(TicketStatuses.Resolved)]
    [InlineData(TicketStatuses.Closed)]
    public void NextStatusOnAssignment_NonOpenTicket_ReturnsNull(string current)
        => Assert.Null(TicketWorkflow.NextStatusOnAssignment(current));
}
