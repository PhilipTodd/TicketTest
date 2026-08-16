using System.Net;
using System.Net.Http.Json;
using TicketTest.Api.Contracts;
using TicketTest.Api.Tests.Infrastructure;

namespace TicketTest.Api.Tests.Tickets;

public sealed class DeleteTicketTests
    : IClassFixture<TicketTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeleteTicketTests(
        TicketTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Delete_WithoutVersion_ReturnsBadRequest()
    {
        var created = await CreateTicketAsync();

        var response = await _client.DeleteAsync(
            $"/api/tickets/{created.Id}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithCurrentVersion_ReturnsNoContent()
    {
        var created = await CreateTicketAsync();

        var response = await _client.DeleteAsync(
            $"/api/tickets/{created.Id}?version={created.Version}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/tickets/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WithStaleVersion_ReturnsConflict() // specified in spec "optimistic-concurrency conflict"
    {
        var created = await CreateTicketAsync();

        var updateRequest = new UpdateTicketRequest
        {
            Title = "Updated before delete",
            Description = created.Description,
            Status = created.Status,
            Priority = created.Priority,
            AssignedTo = created.AssignedTo,
            Version = created.Version
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{created.Id}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/tickets/{created.Id}?version={created.Version}");

        Assert.Equal(
            HttpStatusCode.Conflict,
            deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ClosedTicket_ReturnsBadRequest()
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
            await resolveResponse.Content
                .ReadFromJsonAsync<TicketResponse>();

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
            await closeResponse.Content
                .ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(closed);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/tickets/{created.Id}?version={closed.Version}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownTicket_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync(
            "/api/tickets/999999?version=1");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private async Task<TicketResponse> CreateTicketAsync()
    {
        var request = new CreateTicketRequest
        {
            Title = $"Delete test {Guid.NewGuid()}",
            Description = "Ticket created for delete testing.",
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