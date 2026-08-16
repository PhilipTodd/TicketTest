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
    private static readonly IReadOnlySet<string> AllowedSortFields =
    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedAt",
        "Priority",
        "Title"
    };

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

        if (!AllowedSortFields.Contains(sortBy))
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
            return TicketNotFound(id);
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateTicketFields(
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.AssignedTo);

        if (validationError is not null)
        {
            return validationError;
        }

        if (!TicketRules.IsValidCreateStatus(request.Status))
        {
            return ValidationError(
                "status",
                "A new ticket must start as Open or InProgress.");
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketResponse>> Update(
         int id,
         UpdateTicketRequest request,
         CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .SingleOrDefaultAsync(
                ticket => ticket.Id == id,
                cancellationToken);

        if (ticket is null)
        {
            return TicketNotFound(id);
        }

        if (ticket.Status.Equals(
            "Closed",
            StringComparison.OrdinalIgnoreCase))
        {
            return ValidationError(
                "status",
                "Closed tickets cannot be edited.");
        }

        if (request.Version != ticket.Version)
        {
            return ConcurrencyConflict();
        }

        var validationError = ValidateTicketFields(
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.AssignedTo);

        if (validationError is not null)
        {
            return validationError;
        }

        if (!TicketRules.CanTransition(
            ticket.Status,
            request.Status))
        {
            return ValidationError(
                "status",
                $"Status cannot transition from {ticket.Status} to {request.Status}.");
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Status = NormalizeStatus(request.Status);
        ticket.Priority = NormalizePriority(request.Priority);
        ticket.AssignedTo = NormalizeOptional(request.AssignedTo);
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.Version++;

        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(ticket));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
         int id,
         [FromQuery] int? version,
         CancellationToken cancellationToken)
    {
        if (version is null)
        {
            return ValidationError(
                "version",
                "Version is required.");
        }

        var ticket = await db.Tickets
            .SingleOrDefaultAsync(
                ticket => ticket.Id == id,
                cancellationToken);

        if (ticket is null)
        {
            return TicketNotFound(id);
        }

        if (ticket.Status.Equals(
            "Closed",
            StringComparison.OrdinalIgnoreCase))
        {
            return ValidationError(
                "status",
                "Closed tickets cannot be deleted.");
        }

        if (version.Value != ticket.Version)
        {
            return ConcurrencyConflict();
        }

        db.Tickets.Remove(ticket);

        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // Helper methods:
    private static BadRequestObjectResult? ValidateTicketFields(
        string title,
        string description,
        string status,
        string priority,
        string? assignedTo)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return ValidationError(
                "title",
                "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return ValidationError(
                "description",
                "Description is required.");
        }

        if (!TicketRules.IsValidStatus(status))
        {
            return ValidationError(
                "status",
                "Status must be one of: Open, InProgress, Resolved or Closed.");
        }

        if (!TicketRules.IsValidPriority(priority))
        {
            return ValidationError(
                "priority",
                "Priority must be one of: Low, Medium, High or Critical.");
        }

        if (TicketRules.RequiresAssignee(priority) &&
            string.IsNullOrWhiteSpace(assignedTo))
        {
            return ValidationError(
                "assignedTo",
                "Critical tickets must have an assignee.");
        }

        return null;
    }

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

    private static NotFoundObjectResult TicketNotFound(int id)
    {
        return new NotFoundObjectResult(new ProblemDetails
        {
            Title = "Ticket not found",
            Detail = $"Ticket {id} was not found.",
            Status = StatusCodes.Status404NotFound
        });
    }

    private static ConflictObjectResult ConcurrencyConflict()
    {
        return new ConflictObjectResult(new ProblemDetails
        {
            Title = "Concurrency conflict",
            Detail =
                "The ticket has been modified since it was loaded. " +
                "Reload the ticket and try again.",
            Status = StatusCodes.Status409Conflict
        });
    }

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
