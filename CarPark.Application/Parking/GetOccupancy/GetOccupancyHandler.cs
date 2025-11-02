using CarPark.Domain.Aggregates.ParkingLot;
using MediatR;

namespace CarPark.Application.Parking.GetOccupancy
{

    public sealed class GetOccupancyHandler : IRequestHandler<GetOccupancyQuery, GetOccupancyResult>
    {
        private readonly IParkingLotRepository _repo;

        public GetOccupancyHandler(IParkingLotRepository repo) => _repo = repo;

        public async Task<GetOccupancyResult> Handle(GetOccupancyQuery req, CancellationToken ct)
        {
            var lot = await _repo.GetAsync(ct);

            var available = lot.AvailableSpaces;
            var occupied = lot.OccupiedSpaces;

            return new GetOccupancyResult(available, occupied);
        }
    }
}
