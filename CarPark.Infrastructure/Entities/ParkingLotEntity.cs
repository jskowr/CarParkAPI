using System.ComponentModel.DataAnnotations;

namespace CarPark.Infrastructure.Entities
{
    public sealed class ParkingLotEntity
    {
        [Key]
        public Guid Id { get; set; }
        public int Capacity { get; set; }
        public ICollection<SpaceEntity> Spaces { get; set; } = new List<SpaceEntity>();
        public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
    }
}
