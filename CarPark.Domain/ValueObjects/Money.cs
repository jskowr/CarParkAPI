namespace CarPark.Domain.ValueObjects
{
    public sealed record Money(decimal Amount, string Currency = "GBP")
    {
    }
}
