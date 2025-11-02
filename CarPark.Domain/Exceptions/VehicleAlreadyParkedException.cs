using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Exceptions
{
    public sealed class VehicleAlreadyParkedException : DomainException
    {
        public VehicleAlreadyParkedException(string vehicleReg)
            : base($"Vehicle '{vehicleReg}' is already parked.") { }
    }
}
