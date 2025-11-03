using CarPark.Application.Common;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Services;
using CarPark.Domain.ValueObjects;
using MediatR;

namespace CarPark.Application.Parking.ParkVehicle
{
    public sealed class ParkVehicleCommandHandler : IRequestHandler<ParkVehicleCommand, ParkVehicleResult>
    {
        private readonly IParkingLotRepository _repo;
        private readonly IClock _clock;
        private readonly IMediator _mediator;

        public ParkVehicleCommandHandler(IParkingLotRepository repo, IClock clock, IMediator mediator)
            => (_repo, _clock, _mediator) = (repo, clock, mediator);

        public async Task<ParkVehicleResult> Handle(ParkVehicleCommand command, CancellationToken ct)
        {
            if (!Enum.TryParse<VehicleSize>(command.VehicleSize, true, out var type))
                throw new Exception($"Unknown VehicleType '{command.VehicleSize}'.");

            var lot = await _repo.GetAsync(ct);

            var ticket = lot.Park(new VehicleReg(command.VehicleReg), type, _clock.UtcNow);

            await _repo.SaveAsync(lot, ticket, ct);                               
            await DomainEventPublisher.PublishAndClearAsync(_mediator, ct, lot);

            return new(ticket.VehicleReg.Value, ticket.Space.Number.Value, ticket.TimeInUtc);
        }
    }
}
