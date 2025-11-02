namespace CarPark.API.Contracts.ExitVehicle
{
    public sealed class ExitVehicleResponse
    {
        public string VehicleReg { get; init; } = default!;
        public double VehicleCharge { get; init; }
        public DateTime TimeIn { get; init; }
        public DateTime TimeOut { get; init; }
    }
}
