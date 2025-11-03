using CarPark.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Infrastructure
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
        {
            if (await db.ParkingLots.AnyAsync(ct))
                return;

            var lot = new ParkingLotEntity
            {
                Id = Guid.Parse("dcb56b06-eb82-4f9e-a45d-8d4ab8fdbfe8"),
                Capacity = 3,
                Tickets = new List<TicketEntity>(),
                Spaces = new List<SpaceEntity>()
            };

            for (int i = 1; i <= lot.Capacity; i++)
            {
                var space = new SpaceEntity
                {
                    Id = Guid.NewGuid(),
                    SpaceNumber = i,
                    OccupiedByRegistration = null
                };

                lot.Spaces.Add(space);
            }

            db.ParkingLots.Add(lot);
            await db.SaveChangesAsync(ct);
        }
    }
}
