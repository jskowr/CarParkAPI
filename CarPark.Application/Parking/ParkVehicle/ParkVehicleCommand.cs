using MediatR;

namespace CarPark.Application.Parking.ParkVehicle
{
    public sealed record ParkVehicleCommand(string VehicleReg, string VehicleSize)
        : IRequest<ParkVehicleResult>;
}
