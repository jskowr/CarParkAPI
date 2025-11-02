using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Exceptions
{
    public sealed class VehicleNotFoundException : DomainException
    {
        public VehicleNotFoundException(string vehicleReg)
            : base($"Vehicle '{vehicleReg}' is not currently parked.") { }
    }
}
