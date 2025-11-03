using CarPark.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Infrastructure
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingLotEntity> ParkingLots => Set<ParkingLotEntity>();
        public DbSet<TicketEntity> Tickets => Set<TicketEntity>();
        public DbSet<SpaceEntity> Spaces => Set<SpaceEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParkingLotEntity>(b =>
            {
                b.HasKey(x => x.Id);

                b.HasMany(x => x.Spaces)
                 .WithOne(x => x.ParkingLot)
                 .HasForeignKey(x => x.ParkingLotId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(x => x.Tickets)
                 .WithOne(x => x.ParkingLot)
                 .HasForeignKey(x => x.ParkingLotId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SpaceEntity>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.ParkingLotId, x.SpaceNumber }).IsUnique();

                b.HasMany(s => s.Tickets)
                 .WithOne(t => t.Space)
                 .HasForeignKey(t => t.SpaceId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TicketEntity>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Registration).IsRequired();
                b.Property(x => x.VehicleSize).IsRequired();
            });
        }
    }
}
