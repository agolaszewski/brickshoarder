using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BricksHoarder.MassTransit
{
    public class MassTransitDbContext : DbContext
    {
        public MassTransitDbContext(DbContextOptions<MassTransitDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}