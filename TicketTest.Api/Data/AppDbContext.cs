using Microsoft.EntityFrameworkCore;
using TicketTest.Api.Models;

namespace TicketTest.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>()
            .Property(ticket => ticket.Version)
            .HasDefaultValue(1);
    }
}
