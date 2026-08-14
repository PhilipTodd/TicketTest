using Microsoft.EntityFrameworkCore;
using TicketTest.Api.Models;

namespace TicketTest.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
}
