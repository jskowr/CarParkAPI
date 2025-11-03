using CarPark.Domain.Abstractions;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Events;
using CarPark.Domain.Exceptions;
using CarPark.Domain.Services;
using CarPark.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace Parking.Domain.Aggregates.ParkingLot;

public sealed class ParkingLot : AggregateRootBase
{
    private readonly List<Space> _spaces = new();
    private readonly Dictionary<string, Ticket> _byReg = new(StringComparer.OrdinalIgnoreCase);

    public Guid Id { get; }
    public int Capacity => _spaces.Count;
    public int OccupiedSpaces => _spaces.Count(s => s.IsOccupied);
    public int AvailableSpaces => Capacity - OccupiedSpaces;
    public IReadOnlyList<Space> Spaces => _spaces;

    private ParkingLot(Guid id, IEnumerable<Space> spaces)
    {
        Id = id;
        _spaces.AddRange(spaces);
    }

    public static ParkingLot Create(Guid id, int capacity)
    {
        if (capacity <= 0) throw new ValidationException("Capacity must be positive.");

        var spaces = Enumerable.Range(1, capacity).Select(Space.CreateFree).ToList();
        return new ParkingLot(id, spaces);
    }

    public static ParkingLot Create(Guid id, IEnumerable<Space> spaces)
    {
        var list = spaces?.ToList() ?? throw new ArgumentNullException(nameof(spaces));
        if (list.Count == 0) throw new ValidationException("Parking lot must have at least one space.");
        return new ParkingLot(id, list);
    }

    public static ParkingLot Rehydrate(Guid id, IEnumerable<Space> spaces, IEnumerable<Ticket> tickets)
    {
        var lot = new ParkingLot(id, spaces);
        foreach (var t in tickets.Where(t => t.TimeOutUtc == null))
        {
            lot._byReg[t.VehicleReg.Value] = t;
        }
        return lot;
    }

    public Ticket Park(VehicleReg reg, VehicleSize type, DateTime utcNow)
    {
        if (!Enum.IsDefined(typeof(VehicleSize), type))
            throw new ValidationException($"Unknown vehicle type '{type}'.");

        if (_byReg.ContainsKey(reg.Value))
            throw new VehicleAlreadyParkedException(reg.Value);

        var freeSpace = FindNextFreeSpace();
        if (freeSpace is null)
            throw new NoSpacesAvailableException();

        freeSpace.Occupy(reg.Value);

        var ticket = Ticket.Start(reg, type, freeSpace, utcNow);
        _byReg[reg.Value] = ticket;

        AddDomainEvent(new VehicleParked(Id, reg.Value, freeSpace.Number.Value, utcNow));
        return ticket;
    }

    public ExitResult Exit(VehicleReg reg, DateTime utcNow, IPricingPolicy pricing)
    {
        if (!_byReg.TryGetValue(reg.Value, out var ticket))
            throw new VehicleNotFoundException(reg.Value);

        ticket.Close(utcNow);
        var charge = pricing.Calculate(ticket.TimeInUtc, ticket.TimeOutUtc!.Value, ticket.VehicleSize);

        var space = _spaces.FirstOrDefault(s => s.Number == ticket.Space.Number);
        if (space is null)
            throw new InvalidOperationException($"Internal error: space '{ticket.Space.Number.Value}' not found.");
        if (!space.IsOccupied)
            throw new InvalidOperationException($"Internal error: space '{ticket.Space.Number.Value}' is already free.");

        space.Vacate();

        _byReg.Remove(reg.Value);

        AddDomainEvent(new VehicleExited(
            Id,
            reg.Value,
            ticket.Space.Number.Value,
            ticket.TimeInUtc,
            ticket.TimeOutUtc!.Value,
            charge));

        return new ExitResult(reg.Value, charge, ticket);
    }

    public (int available, int occupied) Snapshot() => (AvailableSpaces, OccupiedSpaces);

    private Space? FindNextFreeSpace()
    {
        return _spaces
            .Where(s => !s.IsOccupied)
            .OrderBy(s => s.Number.Value)
            .FirstOrDefault();
    }
}

public sealed record ExitResult(string VehicleReg, Money VehicleCharge, Ticket Ticket);
