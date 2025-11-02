namespace CarPark.API.Contracts.ParkVehicle
{
    public sealed class ParkVehicleRequest
    {
        public string VehicleReg { get; init; } = default!;
        public string VehicleType { get; init; } = default!;
    }
}
