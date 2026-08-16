using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TicketTest.Api.Contracts;
using TicketTest.Api.Tests.Infrastructure;

namespace TicketTest.Api.Tests.Tickets;

public sealed class UpdateTicketTests
    : IClassFixture<TicketTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UpdateTicketTests(
        TicketTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Update_WithValidChanges_ReturnsOk()
    {
        var created = await CreateTicketAsync();

        var request = new UpdateTicketRequest
        {
            Title = "Updated title",
            Description = created.Description,
            Status = "InProgress",
            Priority = created.Priority,
            AssignedTo = created.AssignedTo,
            Version = created.Version
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated =
            await response.Content.ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(updated);
        Assert.Equal("Updated title", updated.Title);
        Assert.Equal("InProgress", updated.Status);
        Assert.Equal(created.Version + 1, updated.Version);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task Update_WithIllegalStatusTransition_ReturnsBadRequest()
    {
        var created = await CreateTicketAsync();

        var request = new UpdateTicketRequest
        {
            Title = created.Title,
            Description = created.Description,
            Status = "Closed",
            Priority = created.Priority,
            AssignedTo = created.AssignedTo,
            Version = created.Version
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Update_WithStaleVersion_ReturnsConflict()
    {
        var created = await CreateTicketAsync();

        var firstUpdate = new UpdateTicketRequest
        {
            Title = "First update",
            Description = created.Description,
            Status = "Open",
            Priority = created.Priority,
            AssignedTo = created.AssignedTo,
            Version = created.Version
        };

        var firstResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            firstUpdate);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var staleUpdate = new UpdateTicketRequest
        {
            Title = "Stale update",
            Description = created.Description,
            Status = "Open",
            Priority = created.Priority,
            AssignedTo = created.AssignedTo,
            Version = created.Version
        };

        var staleResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            staleUpdate);

        var problem = await staleResponse.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Concurrency conflict", problem.Title);
    }

    [Fact]
    public async Task Update_ClosedTicket_ReturnsBadRequest() // specified in spec "Closed ticket modification"
    {
        var created = await CreateTicketAsync();

        var resolveRequest = new UpdateTicketRequest
        {
            Title = created.Title,
            Description = created.Description,
            Status = "Resolved",
            Priority = created.Priority,
            AssignedTo = created.AssignedTo,
            Version = created.Version
        };

        var resolveResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            resolveRequest);

        var resolved =
            await resolveResponse.Content.ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(resolved);

        var closeRequest = new UpdateTicketRequest
        {
            Title = resolved.Title,
            Description = resolved.Description,
            Status = "Closed",
            Priority = resolved.Priority,
            AssignedTo = resolved.AssignedTo,
            Version = resolved.Version
        };

        var closeResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            closeRequest);

        var closed =
            await closeResponse.Content.ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(closed);

        var editClosedRequest = new UpdateTicketRequest
        {
            Title = "Should fail",
            Description = closed.Description,
            Status = closed.Status,
            Priority = closed.Priority,
            AssignedTo = closed.AssignedTo,
            Version = closed.Version
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            editClosedRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    private async Task<TicketResponse> CreateTicketAsync()
    {
        var request = new CreateTicketRequest
        {
            Title = $"Test ticket {Guid.NewGuid()}",
            Description = "Ticket created for update testing.",
            Status = "Open",
            Priority = "Medium"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/tickets",
            request);

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<TicketResponse>())!;
    }
}