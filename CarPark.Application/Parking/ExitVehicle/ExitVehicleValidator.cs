using FluentValidation;

namespace CarPark.Application.Parking.ExitVehicle
{
    public sealed class ExitVehicleValidator : AbstractValidator<ExitVehicleCommand>
    {
        public ExitVehicleValidator()
            => RuleFor(x => x.VehicleReg).NotEmpty().MaximumLength(16);
    }
}
