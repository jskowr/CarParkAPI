using FluentValidation;

namespace CarPark.Application.Parking.ParkVehicle
{
    public sealed class ParkVehicleCommandValidator : AbstractValidator<ParkVehicleCommand>
    {
        public ParkVehicleCommandValidator()
        {
            RuleFor(x => x.VehicleReg).NotEmpty().MaximumLength(16);
            RuleFor(x => x.VehicleSize).NotEmpty();
        }
    }
}
