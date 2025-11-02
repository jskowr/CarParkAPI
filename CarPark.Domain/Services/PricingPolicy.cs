using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.ValueObjects;

namespace CarPark.Domain.Services
{
    public sealed class PricingPolicy : IPricingPolicy
    {
        private readonly PricingTable _table;

        public PricingPolicy(PricingTable table) => _table = table;

        public Money Calculate(DateTime timeInUtc, DateTime timeOutUtc, VehicleSize vehicleType)
        {
            if (timeOutUtc < timeInUtc)
                throw new ArgumentException("timeOutUtc must be >= timeInUtc");

            var total = timeOutUtc - timeInUtc;

            var minutes = (int)Math.Ceiling(total.TotalMinutes);

            if (minutes <= 0) return new Money(0);

            var perMinuteRate = _table.PerMinute(vehicleType);
            var perMinuteTotal = perMinuteRate * minutes;

            var blocks = minutes / _table.SurchargeBlockMinutes;
            var surchargeTotal = blocks * _table.SurchargePerBlock;

            var amount = perMinuteTotal + surchargeTotal;

            return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero));
        }
    }
}
