namespace TKT.Core.Domain;

public static class TicketWorkflow
{
    private static readonly Dictionary<string, HashSet<string>> Transitions = new()
    {
        [TicketStatuses.Open] = [TicketStatuses.InProgress],
        [TicketStatuses.InProgress] = [TicketStatuses.Pending, TicketStatuses.Resolved],
        [TicketStatuses.Pending] = [TicketStatuses.InProgress, TicketStatuses.Resolved],
        [TicketStatuses.Resolved] = [TicketStatuses.Closed, TicketStatuses.InProgress],
        [TicketStatuses.Closed] = [TicketStatuses.InProgress],
    };

    public static bool IsKnownStatus(string status)
        => Transitions.ContainsKey(status);

    public static bool CanTransition(string from, string to)
        => from == to || (Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to));

    public static string? NextStatusOnComment(string currentStatus, bool authorIsStaff, bool isInternal)
    {
        if (isInternal)
            return null;

        return (currentStatus, authorIsStaff) switch
        {
            (TicketStatuses.InProgress, true) => TicketStatuses.Pending,
            (TicketStatuses.Pending, false) => TicketStatuses.InProgress,
            _ => null,
        };
    }

    public static string? NextStatusOnAssignment(string currentStatus)
        => currentStatus == TicketStatuses.Open ? TicketStatuses.InProgress : null;
}
