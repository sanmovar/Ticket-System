using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ticket_System.Models;

namespace Ticket_System.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<TicketAbhaengigkeit> TicketAbhaengigkeiten { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Beziehung Ticket–Comment: kein Cascade Delete (verhindert multiple cascade paths)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketAbhaengigkeit>(entity =>
            {
                entity.HasKey(e => new { e.BlockiertesTicketId, e.BlockierendesTicketId });

                entity.HasOne(e => e.BlockiertesTicket)
                      .WithMany(t => t.WirdBlockiertDurch)
                      .HasForeignKey(e => e.BlockiertesTicketId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BlockierendesTicket)
                      .WithMany(t => t.BlockiertAndere)
                      .HasForeignKey(e => e.BlockierendesTicketId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }


    }
}
