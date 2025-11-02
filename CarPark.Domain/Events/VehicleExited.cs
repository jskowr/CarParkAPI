using CarPark.Domain.ValueObjects;

namespace CarPark.Domain.Events
{
    public sealed record VehicleExited(
        Guid LotId,
        string VehicleReg,
        int SpaceNumber,
        DateTime TimeInUtc,
        DateTime TimeOutUtc,
        Money Charge)
        : DomainEvent;
}
