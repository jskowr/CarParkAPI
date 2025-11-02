namespace CarPark.Application.Parking.ExitVehicle
{ 
    public sealed record ExitVehicleResult(string VehicleReg, decimal VehicleCharge, DateTime TimeInUtc, DateTime TimeOutUtc);
}
