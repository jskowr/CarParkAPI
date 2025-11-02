namespace CarPark.API.Contracts.ParkVehicle
{
    public sealed class ParkVehicleResponse
    {
        public string VehicleReg { get; init; } = default!;
        public int SpaceNumber { get; init; }
        public DateTime TimeIn { get; init; }
    }
}
