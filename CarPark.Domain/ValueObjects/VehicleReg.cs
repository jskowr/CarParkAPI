using CarPark.Domain.Exceptions;

namespace CarPark.Domain.ValueObjects
{
    public sealed record VehicleReg
    {
        public string Value { get; }
        public VehicleReg(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ValidationException("Vehicle registration cannot be empty.");

            if (value.Length > 16)
                throw new ValidationException("Vehicle registration cannot exceed 16 characters.");

            Value = value.Trim();
        }
        public override string ToString() => Value;
    }
}
