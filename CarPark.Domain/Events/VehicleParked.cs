using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Events
{
    public sealed record VehicleParked(
        Guid LotId,
        string VehicleReg,
        int SpaceNumber,
        DateTime TimeInUtc)
        : DomainEvent;
}
