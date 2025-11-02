using MediatR;

namespace CarPark.Application.Parking.ExitVehicle
{
    public sealed record ExitVehicleCommand(string VehicleReg) : IRequest<ExitVehicleResult>;
}
