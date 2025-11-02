using CarPark.Domain.Aggregates.ParkingLot;

namespace CarPark.Domain.Services
{
    public sealed record PricingTable(
        decimal SmallPerMinute,
        decimal MediumPerMinute,
        decimal LargePerMinute,
        int SurchargeBlockMinutes,
        decimal SurchargePerBlock)
    {
        public decimal PerMinute(VehicleSize size) => size switch
        {
            VehicleSize.SMALL => SmallPerMinute,
            VehicleSize.MEDIUM => MediumPerMinute,
            VehicleSize.LARGE => LargePerMinute,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, "Unknown vehicle size")
        };
    }
}
