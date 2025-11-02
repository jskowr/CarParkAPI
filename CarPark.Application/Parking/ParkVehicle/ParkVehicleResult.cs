namespace CarPark.Application.Parking.ParkVehicle
{
    public sealed record ParkVehicleResult(string VehicleReg, int SpaceNumber, DateTime TimeInUtc);
}
