using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketTest.Api.Data;
using TicketTest.Api.Models;

namespace TicketTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Ticket>>> GetAll([FromQuery] string? status)
    {
        var query = db.Tickets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetById(int id)
    {
        var ticket = await db.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> Create(Ticket ticket)
    {
        ticket.Id = 0;
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = null;

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
    }
}
