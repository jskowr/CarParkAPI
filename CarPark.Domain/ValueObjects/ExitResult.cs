namespace CarPark.Domain.ValueObjects
{
    public sealed record ExitResult(string VehicleReg, Money VehicleCharge, DateTime TimeInUtc, DateTime TimeOutUtc);
}
