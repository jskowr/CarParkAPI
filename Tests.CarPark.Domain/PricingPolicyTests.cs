using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Services;
using FluentAssertions;

namespace Tests.CarPark.Domain
{
    public sealed class PricingPolicyTests
    {
        private static PricingPolicy CreatePolicy() =>
            new PricingPolicy(new PricingTable(
                SmallPerMinute: 0.10m,
                MediumPerMinute: 0.20m,
                LargePerMinute: 0.40m,
                SurchargeBlockMinutes: 5,
                SurchargePerBlock: 1.00m
            ));

        private static (DateTime In, DateTime Out) SpanFromMinutes(int minutes)
        {
            var t0 = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            return (t0, t0.AddMinutes(minutes));
        }

        [Theory]
        [InlineData(1, VehicleSize.SMALL, 0.10)]
        [InlineData(5, VehicleSize.MEDIUM, 2.00)]
        [InlineData(6, VehicleSize.LARGE, 3.40)]
        [InlineData(9, VehicleSize.SMALL, 1.90)]
        [InlineData(10, VehicleSize.MEDIUM, 4.00)]
        public void Calculate_ShouldMatchExamples(int minutes, VehicleSize size, double expected)
        {
            // Arrange
            var policy = CreatePolicy();
            var (tin, tout) = SpanFromMinutes(minutes);

            // Act
            var money = policy.Calculate(tin, tout, size);

            // Assert
            decimal expectedDec = Math.Round((decimal)expected, 2, MidpointRounding.AwayFromZero);
            money.Amount.Should().Be(expectedDec);
        }

        [Fact]
        public void Calculate_ShouldRoundUpToNextFullMinute()
        {
            var policy = CreatePolicy();
            var t0 = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var tin = t0;
            var tout = t0.AddSeconds(6);

            // Act
            var money = policy.Calculate(tin, tout, VehicleSize.SMALL);

            // Assert
            money.Amount.Should().Be(0.10m);
        }

        [Fact]
        public void Calculate_ShouldReturnZero_ForZeroDuration()
        {
            // Arrange
            var policy = CreatePolicy();
            var t0 = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

            // Act
            var money = policy.Calculate(t0, t0, VehicleSize.SMALL);

            // Assert
            money.Amount.Should().Be(0m);
        }

        [Fact]
        public void Calculate_ShouldThrow_ForNegativeDuration()
        {
            // Arrange
            var policy = CreatePolicy();
            var t0 = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var tin = t0.AddMinutes(5);
            var tout = t0;

            // Act
            var act = () => policy.Calculate(tin, tout, VehicleSize.MEDIUM);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*timeOutUtc must be >= timeInUtc*");
        }

        [Theory]
        [InlineData(4, VehicleSize.LARGE, 1.60)]
        [InlineData(5, VehicleSize.LARGE, 3.00)]
        [InlineData(11, VehicleSize.LARGE, 6.40)]
        public void Calculate_ShouldApplySurchargePerFull5MinBlock(int minutes, VehicleSize size, double expected)
        {
            // Arrange
            var policy = CreatePolicy();
            var (tin, tout) = SpanFromMinutes(minutes);

            // Act
            var money = policy.Calculate(tin, tout, size);

            // Assert
            money.Amount.Should().Be(Math.Round((decimal)expected, 2, MidpointRounding.AwayFromZero));
        }
    }
}
