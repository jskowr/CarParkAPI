using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Events
{
    public abstract record DomainEvent : IDomainEvent
    {
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
