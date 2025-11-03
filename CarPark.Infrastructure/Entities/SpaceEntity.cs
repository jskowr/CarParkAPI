namespace CarPark.Infrastructure.Entities
{
    public sealed class SpaceEntity
    {
        public Guid Id { get; set; }
        public int SpaceNumber { get; set; }
        public string? OccupiedByRegistration { get; set; }
        public Guid ParkingLotId { get; set; }
        public ParkingLotEntity ParkingLot { get; set; } = default!;
        public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
    }
}
