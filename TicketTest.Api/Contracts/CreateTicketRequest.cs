using System.ComponentModel.DataAnnotations;

namespace TicketTest.Api.Contracts;

public sealed class CreateTicketRequest
{
    [Required]
    [MaxLength(150)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public string Status { get; init; } = "Open";

    [Required]
    public string Priority { get; init; } = "Medium";

    [MaxLength(100)]
    public string? AssignedTo { get; init; }
}
