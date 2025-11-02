using MediatR;
using CarPark.Domain.Abstractions;

namespace CarPark.Application.Common
{
    public static class DomainEventPublisher
    {
        public static async Task PublishAndClearAsync(
            IMediator mediator, CancellationToken ct, params AggregateRootBase[] aggregates)
        {
            foreach (var agg in aggregates)
            {
                var events = agg.DomainEvents.ToArray();
                agg.ClearDomainEvents();
                foreach (var e in events)
                    await mediator.Publish(e, ct);
            }
        }
    }
}
