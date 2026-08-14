namespace TicketTest.Api.Contracts;

public sealed record TicketResponse(
    int Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string? AssignedTo,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Version);
