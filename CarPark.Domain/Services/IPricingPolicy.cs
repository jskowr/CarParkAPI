using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.ValueObjects;

namespace CarPark.Domain.Services
{
    public interface IPricingPolicy
    {
        Money Calculate(DateTime timeInUtc, DateTime timeOutUtc, VehicleSize vehicleSize);
    }
}
