using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using TicketTest.Api.Tests.Infrastructure;

namespace TicketTest.Api.Tests.Tickets;

public sealed class GetTicketTests
    : IClassFixture<TicketTestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetTicketTests(
        TicketTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetById_UnknownTicket_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync(
            "/api/tickets/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Ticket not found", problem.Title);
    }
}