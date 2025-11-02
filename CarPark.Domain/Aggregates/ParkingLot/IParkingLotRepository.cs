using DomainParkingLot = Parking.Domain.Aggregates.ParkingLot.ParkingLot;

namespace CarPark.Domain.Aggregates.ParkingLot
{
    public interface IParkingLotRepository
    {
        Task<DomainParkingLot> GetAsync(CancellationToken ct);
        Task SaveAsync(DomainParkingLot lot, CancellationToken ct);
    }
}
