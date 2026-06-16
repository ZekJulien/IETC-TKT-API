using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Tickets;

public sealed class UpdateTicketRequest
{
    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    public Guid? AssignedTo { get; set; }

    public Guid? CategoryId { get; set; }

    public DateTimeOffset? DueDate { get; set; }
}
