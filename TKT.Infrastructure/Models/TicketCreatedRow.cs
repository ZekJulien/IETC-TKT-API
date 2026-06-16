namespace TKT.Infrastructure.Models;

public sealed class TicketCreatedRow
{
    public string TicketNumber { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
