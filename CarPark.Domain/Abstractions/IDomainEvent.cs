using MediatR;

namespace CarPark.Domain.Abstractions
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOnUtc { get; }
    }
}
