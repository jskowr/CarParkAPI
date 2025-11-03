using CarPark.Domain.Aggregates.ParkingLot;

namespace CarPark.Infrastructure.Entities
{
    public sealed class TicketEntity
    {
        public Guid Id { get; set; }
        public string Registration { get; set; } = default!;
        public VehicleSize VehicleSize { get; set; }
        public DateTime EnteredAtUtc { get; set; }
        public DateTime? ExitedAtUtc { get; set; }
        public Guid SpaceId { get; set; }
        public SpaceEntity Space { get; set; } = default!;
        public Guid ParkingLotId { get; set; }
        public ParkingLotEntity ParkingLot { get; set; } = default!;
    }
}
