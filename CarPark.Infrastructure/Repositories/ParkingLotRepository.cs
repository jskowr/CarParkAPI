using CarPark.Domain.Aggregates.ParkingLot;
using Parking.Domain.Aggregates.ParkingLot;

namespace CarPark.Infrastructure.Repositories
{
    public sealed class ParkingLotRepository : IParkingLotRepository
    {
        private static readonly Dictionary<Guid, ParkingLot> _lots = new();

        static ParkingLotRepository()
        {
            var seededId = Guid.Parse("dcb56b06-eb82-4f9e-a45d-8d4ab8fdbfe8");
            var seededLot = ParkingLot.Create(seededId, capacity: 3);
            _lots[seededId] = seededLot;
        }

        public async Task<ParkingLot> GetAsync(CancellationToken ct)
        {
            return _lots.First().Value;
        }

        public async Task SaveAsync(ParkingLot lot, CancellationToken ct)
        {
            _lots[lot.Id] = lot;
        }

        public IReadOnlyCollection<ParkingLot> GetAll() => _lots.Values.ToList();
    }
}
