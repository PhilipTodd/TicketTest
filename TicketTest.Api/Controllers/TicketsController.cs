using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketTest.Api.Contracts;
using TicketTest.Api.Data;
using TicketTest.Api.Models;
using TicketTest.Api.Services;

namespace TicketTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TicketResponse>>> GetAll(
    [FromQuery] string? status,
    [FromQuery] string? priority,
    [FromQuery] string? assignedTo,
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string sortBy = "CreatedAt",
    [FromQuery] string sortDirection = "desc",
    CancellationToken cancellationToken = default)
    {
        if (page < 1) // no negative page numer
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid page",
                Detail = "Page must be greater than or equal to 1.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (pageSize < 1 || pageSize > 100) // page size between 1 and 100
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid page size",
                Detail = "Page size must be between 1 and 100.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                !sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)) // sort dsirection must be either asc or desc
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid sort direction",
                Detail = "Sort direction must be 'asc' or 'desc'.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var allowedSortFields = new[] { "CreatedAt", "Priority", "Title" };

        if (!allowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase)) // sort field not allowed
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid sort field",
                Detail = "Sort field must be CreatedAt, Priority or Title.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var query = db.Tickets
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(ticket => ticket.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            query = query.Where(ticket => ticket.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(assignedTo))
        {
            query = query.Where(ticket => ticket.AssignedTo == assignedTo);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ticket =>
                ticket.Title.Contains(search) ||
                ticket.Description.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var descending = sortDirection.Equals(
            "desc",
            StringComparison.OrdinalIgnoreCase);

        query = sortBy.ToLowerInvariant() switch
        {
            "title" => descending
                ? query.OrderByDescending(ticket => ticket.Title)
                : query.OrderBy(ticket => ticket.Title),

            "priority" => descending
                ? query.OrderByDescending(ticket =>
                    ticket.Priority == "Critical" ? 4 :
                    ticket.Priority == "High" ? 3 :
                    ticket.Priority == "Medium" ? 2 : 1)
                : query.OrderBy(ticket =>
                    ticket.Priority == "Critical" ? 4 :
                    ticket.Priority == "High" ? 3 :
                    ticket.Priority == "Medium" ? 2 : 1), // when sorting by priority give numeric values for proper sorting Critical -> High -> Medium -> Low

            "createdat" => descending
                ? query.OrderByDescending(ticket => ticket.CreatedAt)
                : query.OrderBy(ticket => ticket.CreatedAt),

            _ => query.OrderByDescending(ticket => ticket.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ticket => new TicketResponse(
                ticket.Id,
                ticket.Title,
                ticket.Description,
                ticket.Status,
                ticket.Priority,
                ticket.AssignedTo,
                ticket.CreatedAt,
                ticket.UpdatedAt,
                ticket.Version))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedResponse<TicketResponse>(
            items,
            page,
            pageSize,
            totalCount,
            totalPages));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.Id == id)
            .Select(ticket => new TicketResponse(
                ticket.Id,
                ticket.Title,
                ticket.Description,
                ticket.Status,
                ticket.Priority,
                ticket.AssignedTo,
                ticket.CreatedAt,
                ticket.UpdatedAt,
                ticket.Version))
            .SingleOrDefaultAsync(cancellationToken);

        if (ticket is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Ticket not found",
                Detail = $"Ticket {id} was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(
    CreateTicketRequest request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ValidationError(
                "title",
                "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return ValidationError(
                "description",
                "Description is required.");
        }

        if (!TicketRules.IsValidStatus(request.Status))
        {
            return ValidationError(
                "status",
                "Status must be one of: Open, InProgress, Resolved or Closed.");
        }

        if (!TicketRules.IsValidCreateStatus(request.Status))
        {
            return ValidationError(
                "status",
                "A new ticket must start as Open or InProgress.");
        }

        if (!TicketRules.IsValidPriority(request.Priority))
        {
            return ValidationError(
                "priority",
                "Priority must be one of: Low, Medium, High or Critical.");
        }

        if (TicketRules.RequiresAssignee(request.Priority) &&
            string.IsNullOrWhiteSpace(request.AssignedTo))
        {
            return ValidationError(
                "assignedTo",
                "Critical tickets must have an assignee.");
        }

        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = NormalizeStatus(request.Status),
            Priority = NormalizePriority(request.Priority),
            AssignedTo = NormalizeOptional(request.AssignedTo),
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };

        db.Tickets.Add(ticket);

        await db.SaveChangesAsync(cancellationToken);

        var response = ToResponse(ticket);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticket.Id },
            response);
    }


    // Helper methods:
    private static BadRequestObjectResult ValidationError(
        string field,
        string message)
    {
        return new BadRequestObjectResult(
            new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [field] = [message]
                })
            {
                Status = StatusCodes.Status400BadRequest
            });
    }

    private static TicketResponse ToResponse(Ticket ticket) =>
    new(
        ticket.Id,
        ticket.Title,
        ticket.Description,
        ticket.Status,
        ticket.Priority,
        ticket.AssignedTo,
        ticket.CreatedAt,
        ticket.UpdatedAt,
        ticket.Version);

    private static string NormalizeStatus(string status) =>
        status.ToLowerInvariant() switch
        {
            "open" => "Open",
            "inprogress" => "InProgress",
            "resolved" => "Resolved",
            "closed" => "Closed",
            _ => status
        };

    private static string NormalizePriority(string priority) =>
        priority.ToLowerInvariant() switch
        {
            "low" => "Low",
            "medium" => "Medium",
            "high" => "High",
            "critical" => "Critical",
            _ => priority
        };

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
