using CarPark.Application.Common;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Services;
using CarPark.Domain.ValueObjects;
using MediatR;

namespace CarPark.Application.Parking.ExitVehicle
{
    public sealed class ExitVehicleCommandHandler : IRequestHandler<ExitVehicleCommand, ExitVehicleResult>
    {
        private readonly IParkingLotRepository _repo;
        private readonly IPricingPolicy _pricing;
        private readonly IClock _clock;
        private readonly IMediator _mediator;

        public ExitVehicleCommandHandler(IParkingLotRepository repo, IPricingPolicy pricing, IClock clock, IMediator mediator)
            => (_repo, _pricing, _clock, _mediator) = (repo, pricing, clock, mediator);

        public async Task<ExitVehicleResult> Handle(ExitVehicleCommand req, CancellationToken ct)
        {
            var lot = await _repo.GetAsync(ct);

            var exit = lot.Exit(new VehicleReg(req.VehicleReg), _clock.UtcNow, _pricing);

            await _repo.SaveAsync(lot, exit.Ticket, ct);

            await DomainEventPublisher.PublishAndClearAsync(_mediator, ct, lot);

            return new ExitVehicleResult(
                VehicleReg: exit.VehicleReg,
                VehicleCharge: exit.VehicleCharge.Amount,
                TimeInUtc: exit.Ticket.TimeInUtc,
                TimeOutUtc: exit.Ticket.TimeOutUtc!.Value);
        }
    }
}
