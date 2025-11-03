using CarPark.Domain.Exceptions;
using CarPark.Domain.ValueObjects;

namespace CarPark.Domain.Aggregates.ParkingLot
{
    public sealed class Space
    {
        public SpaceNumber Number { get; }
        public bool IsOccupied { get; private set; }
        public string? OccupiedByRegistration { get; private set; }

        private Space(SpaceNumber number, bool isOccupied, string? reg)
        {
            Number = number;
            IsOccupied = isOccupied;
            OccupiedByRegistration = reg;
        }

        public static Space CreateFree(int number) => new(SpaceNumber.Create(number), false, null);

        public static Space Rehydrate(int number, bool isOccupied, string? reg)
            => new(SpaceNumber.Create(number), isOccupied, reg);

        public void Occupy(string registration)
        {
            if (IsOccupied) throw new SpaceAlreadyOccupied(Number.Value);
            IsOccupied = true;
            OccupiedByRegistration = registration;
        }

        public void Vacate()
        {
            if (!IsOccupied) throw new SpaceAlreadyFree(Number.Value);
            IsOccupied = false;
            OccupiedByRegistration = null;
        }
    }
}
