using System.ComponentModel.DataAnnotations;

namespace TKT.Api.Contracts.Tickets;

public sealed class CreateTicketRequest
{
    [Required, MinLength(3), MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? AssignedTo { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }
}
