using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TicketTest.Api.Contracts;
using TicketTest.Api.Tests.Infrastructure;

namespace TicketTest.Api.Tests.Tickets;

public sealed class CreateTicketTests
    : IClassFixture<TicketTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateTicketTests(
        TicketTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidTicket_ReturnsCreated()
    {
        var request = new CreateTicketRequest
        {
            Title = "Investigate API timeout",
            Description = "Customers are experiencing intermittent timeouts.",
            Status = "Open",
            Priority = "High",
            AssignedTo = "Alex"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/tickets",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var ticket =
            await response.Content.ReadFromJsonAsync<TicketResponse>();

        Assert.NotNull(ticket);
        Assert.Equal(request.Title, ticket.Title);
        Assert.Equal("Open", ticket.Status);
        Assert.Equal("High", ticket.Priority);
        Assert.Equal(1, ticket.Version);
        Assert.True(ticket.Id > 0);
    }

    [Fact]
    public async Task Create_WithResolvedStatus_ReturnsValidationProblem()
    {
        var request = new CreateTicketRequest
        {
            Title = "Resolved ticket",
            Description = "Should not be created resolved.",
            Status = "Resolved",
            Priority = "Medium"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/tickets",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("status", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_CriticalWithoutAssignee_ReturnsBadRequest() // specified in spec "Critical ticket without assignee"
    {
        var request = new CreateTicketRequest
        {
            Title = "Critical failure",
            Description = "Critical ticket without an assignee.",
            Status = "Open",
            Priority = "Critical",
            AssignedTo = null
        };

        var response = await _client.PostAsJsonAsync(
            "/api/tickets",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}