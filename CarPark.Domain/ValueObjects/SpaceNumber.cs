using CarPark.Domain.Exceptions;

namespace CarPark.Domain.ValueObjects
{

    public sealed record SpaceNumber
    {
        public int Value { get; }

        private SpaceNumber(int value) => Value = value;

        public static SpaceNumber Create(int value)
        {
            if (value <= 0)
                throw new ValidationException("Space number must be positive.");

            return new SpaceNumber(value);
        }

        public override string ToString() => Value.ToString();
    }
}
