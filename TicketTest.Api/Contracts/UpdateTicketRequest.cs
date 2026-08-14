using System.ComponentModel.DataAnnotations;

namespace TicketTest.Api.Contracts;

public sealed class UpdateTicketRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public string Status { get; init; } = string.Empty;

    [Required]
    public string Priority { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? AssignedTo { get; init; }

    public int Version { get; init; }
}
