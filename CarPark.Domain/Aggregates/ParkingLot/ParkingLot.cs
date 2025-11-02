using CarPark.Domain.Abstractions;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Events;
using CarPark.Domain.Exceptions;
using CarPark.Domain.Services;
using CarPark.Domain.ValueObjects;

namespace Parking.Domain.Aggregates.ParkingLot;

public sealed class ParkingLot : AggregateRootBase
{
    private readonly SortedSet<int> _freeSpaces;
    private readonly Dictionary<string, Ticket> _byReg = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, Ticket> _bySpace = new();

    public Guid Id { get; }
    public int Capacity { get; }
    public int OccupiedSpaces => _bySpace.Count;
    public int AvailableSpaces => Capacity - OccupiedSpaces;

    private ParkingLot(Guid id, int capacity)
    {
        Id = id;
        Capacity = capacity;
        _freeSpaces = new SortedSet<int>(Enumerable.Range(1, capacity));
    }

    public static ParkingLot Create(Guid id, int capacity)
    {
        if (capacity <= 0) throw new ValidationException("Capacity must be positive.");

        return new ParkingLot(id, capacity);
    }

    public Ticket Park(VehicleReg reg, VehicleSize type, DateTime utcNow)
    {
        if (!Enum.IsDefined(typeof(VehicleSize), type))
            throw new ValidationException($"Unknown vehicle type '{type}'.");

        if (_byReg.ContainsKey(reg.Value))
            throw new VehicleAlreadyParkedException(reg.Value);

        if (_freeSpaces.Count == 0)
            throw new NoSpacesAvailableException();

        var space = _freeSpaces.Min!;
        _freeSpaces.Remove(space);

        var ticket = Ticket.Start(reg, type, SpaceNumber.Create(space), utcNow);
        _byReg[reg.Value] = ticket;
        _bySpace[space] = ticket;

        AddDomainEvent(new VehicleParked(Id, reg.Value, space, utcNow));
        return ticket;
    }

    public ExitResult Exit(VehicleReg reg, DateTime utcNow, IPricingPolicy pricing)
    {
        if (!_byReg.TryGetValue(reg.Value, out var ticket))
            throw new VehicleNotFoundException(reg.Value);

        ticket.Close(utcNow);
        var charge = pricing.Calculate(ticket.TimeInUtc, ticket.TimeOutUtc!.Value, ticket.VehicleType);

        _byReg.Remove(reg.Value);
        _bySpace.Remove(ticket.SpaceNumber.Value);
        _freeSpaces.Add(ticket.SpaceNumber.Value);

        AddDomainEvent(new VehicleExited(Id, reg.Value, ticket.SpaceNumber.Value, ticket.TimeInUtc, ticket.TimeOutUtc!.Value, charge));

        return new ExitResult(reg.Value, charge, ticket.TimeInUtc, ticket.TimeOutUtc!.Value);
    }

    public (int available, int occupied) Snapshot() => (AvailableSpaces, OccupiedSpaces);
}

public sealed record ExitResult(string VehicleReg, Money VehicleCharge, DateTime TimeInUtc, DateTime TimeOutUtc);
