using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.ValueObjects;
using CarPark.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Parking.Domain.Aggregates.ParkingLot;

namespace CarPark.Infrastructure.Repositories
{
    public sealed class ParkingLotRepository : IParkingLotRepository
    {
        private readonly AppDbContext _db;

        public ParkingLotRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ParkingLot> GetAsync(CancellationToken ct)
        {
            // we get here only one parking lot, in real application we could get it by id
            var entity = await _db.ParkingLots
                .Include(x => x.Spaces).ThenInclude(s => s.Tickets)
                .Include(x => x.Tickets)
                .FirstAsync(ct);

            var spaces = entity.Spaces
                .Select(s => Space.Rehydrate(
                    number: s.SpaceNumber,
                    isOccupied: s.OccupiedByRegistration != null,
                    reg: s.OccupiedByRegistration))
                .OrderBy(s => s.Number.Value)
                .ToList();

            if (spaces.Count < entity.Capacity)
            {
                var have = spaces.Select(s => s.Number.Value).ToHashSet();
                for (int i = 1; i <= entity.Capacity; i++)
                {
                    if (!have.Contains(i))
                        spaces.Add(Space.Rehydrate(i, false, null));
                }
                spaces = spaces.OrderBy(s => s.Number.Value).ToList();
            }

            var byNumber = spaces.ToDictionary(s => s.Number.Value);

            var tickets = entity.Tickets.Select(t =>
            {
                var spaceNumber = entity.Spaces.First(s => s.Id == t.SpaceId).SpaceNumber;
                var space = byNumber[spaceNumber];

                return Ticket.Rehydrate(
                    reg: new VehicleReg(t.Registration),
                    type: t.VehicleSize,
                    space: space,
                    inUtc: t.EnteredAtUtc,
                    outUtc: t.ExitedAtUtc);
            }).ToList();

            var lot = ParkingLot.Rehydrate(entity.Id, spaces, tickets);
            return lot;
        }

        public async Task SaveAsync(ParkingLot lot, Ticket ticket, CancellationToken ct)
        {
            var dbLot = await _db.ParkingLots
                .Include(x => x.Spaces).ThenInclude(s => s.Tickets)
                .Include(x => x.Tickets)
                .FirstAsync(ct);

            dbLot.Capacity = lot.Capacity;

            var spaceNumber = ticket.Space.Number.Value;
            var space = dbLot.Spaces.Single(s => s.SpaceNumber == spaceNumber);

            var existingTicket =
                dbLot.Tickets.SingleOrDefault(t =>
                    t.ExitedAtUtc == null &&
                    t.Registration == ticket.VehicleReg.Value)
                ?? space.Tickets.FirstOrDefault(t => t.ExitedAtUtc == null);

            if (ticket.TimeOutUtc is not null)
            {
                if (existingTicket is not null)
                {
                    existingTicket.ExitedAtUtc = ticket.TimeOutUtc;

                    space.OccupiedByRegistration = null;
                }

                await _db.SaveChangesAsync(ct);
                return;
            }

            if (existingTicket is null)
            {
                var newTicket = new TicketEntity
                {
                    Id = Guid.NewGuid(),
                    Registration = ticket.VehicleReg.Value,
                    VehicleSize = ticket.VehicleSize,
                    EnteredAtUtc = ticket.TimeInUtc,
                    ExitedAtUtc = null,
                    SpaceId = space.Id,
                    ParkingLotId = dbLot.Id
                };

                dbLot.Tickets.Add(newTicket);
                _db.Add(newTicket);

                space.OccupiedByRegistration = ticket.VehicleReg.Value;
            }
            else
            {
                existingTicket.Registration = ticket.VehicleReg.Value;
                existingTicket.VehicleSize = ticket.VehicleSize;
                existingTicket.EnteredAtUtc = ticket.TimeInUtc;
                existingTicket.ExitedAtUtc = null;

                if (existingTicket.SpaceId != space.Id)
                {
                    existingTicket.SpaceId = space.Id;
                }

                space.OccupiedByRegistration = ticket.VehicleReg.Value;
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}
