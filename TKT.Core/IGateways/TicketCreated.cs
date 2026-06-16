namespace TKT.Core.IGateways;

public sealed record TicketCreated(string TicketNumber, DateTimeOffset CreatedAt);
