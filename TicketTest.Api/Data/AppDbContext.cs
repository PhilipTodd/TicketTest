using Microsoft.EntityFrameworkCore;
using TicketTest.Api.Models;

namespace TicketTest.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ticketing");

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");

            entity.Property(ticket => ticket.Version)
                .HasDefaultValue(1);
        });
    }
}