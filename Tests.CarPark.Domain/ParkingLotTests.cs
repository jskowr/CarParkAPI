using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Events;
using CarPark.Domain.Exceptions;
using CarPark.Domain.Services;
using CarPark.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Parking.Domain.Aggregates.ParkingLot;

namespace Tests.CarPark.Domain
{
    public sealed class ParkingLotTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_ShouldThrow_WhenCapacityIsNotPositive(int badCapacity)
        {
            // Act
            var act = () => ParkingLot.Create(Guid.NewGuid(), badCapacity);

            // Assert
            act.Should().Throw<ValidationException>()
               .WithMessage("*Capacity must be positive*");
        }

        [Fact]
        public void Park_ShouldAllocateFirstFreeSpace()
        {
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 3);
            var now = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

            // Act
            var t1 = lot.Park(new VehicleReg("AAA111"), VehicleSize.SMALL, now);
            var t2 = lot.Park(new VehicleReg("BBB222"), VehicleSize.MEDIUM, now);

            t1.SpaceNumber.Value.Should().Be(1);
            t2.SpaceNumber.Value.Should().Be(2);
            lot.OccupiedSpaces.Should().Be(2);
            lot.AvailableSpaces.Should().Be(1);
        }

        [Fact]
        public void Park_ShouldThrow_WhenVehicleAlreadyParked()
        {
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 2);
            var now = DateTime.UtcNow;

            lot.Park(new VehicleReg("DUPLICATE"), VehicleSize.SMALL, now);

            var act = () => lot.Park(new VehicleReg("DUPLICATE"), VehicleSize.SMALL, now);

            act.Should().Throw<VehicleAlreadyParkedException>()
               .WithMessage("*already parked*");
        }

        [Fact]
        public void Park_ShouldThrow_WhenNoSpacesAvailable()
        {
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 1);
            var now = DateTime.UtcNow;

            lot.Park(new VehicleReg("CAR-1"), VehicleSize.SMALL, now);
            var act = () => lot.Park(new VehicleReg("CAR-2"), VehicleSize.SMALL, now);

            act.Should().Throw<NoSpacesAvailableException>();
        }

        [Fact]
        public void Exit_ShouldFreeUpSpace_CalculateCharge_AndRaiseEvent()
        {
            // Arrange
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 2);
            var inAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var outAt = new DateTime(2025, 1, 1, 10, 6, 0, DateTimeKind.Utc);

            lot.Park(new VehicleReg("CAR-1"), VehicleSize.SMALL, inAt);
            lot.Park(new VehicleReg("CAR-2"), VehicleSize.MEDIUM, inAt);

            var pricing = new Mock<IPricingPolicy>();
            pricing.Setup(p => p.Calculate(inAt, outAt, VehicleSize.SMALL))
                   .Returns(new Money(3.40m, "GBP"));

            lot.ClearDomainEvents();

            // Act
            var result = lot.Exit(new VehicleReg("CAR-1"), outAt, pricing.Object);

            result.VehicleReg.Should().Be("CAR-1");
            result.TimeInUtc.Should().Be(inAt);
            result.TimeOutUtc.Should().Be(outAt);
            result.VehicleCharge.Amount.Should().Be(3.40m);

            lot.OccupiedSpaces.Should().Be(1);
            lot.AvailableSpaces.Should().Be(1);

            var t3 = lot.Park(new VehicleReg("CAR-3"), VehicleSize.LARGE, outAt);
            t3.SpaceNumber.Value.Should().Be(1);

            pricing.Verify(p => p.Calculate(inAt, outAt, VehicleSize.SMALL), Times.Once);
        }

        [Fact]
        public void Exit_ShouldThrow_WhenVehicleNotFound()
        {
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 2);
            var pricing = new Mock<IPricingPolicy>();

            var act = () => lot.Exit(new VehicleReg("MISSING"), DateTime.UtcNow, pricing.Object);

            act.Should().Throw<VehicleNotFoundException>()
               .WithMessage("*not currently parked*");
            pricing.VerifyNoOtherCalls();
        }

        [Fact]
        public void Snapshot_ShouldReturn_CurrentAvailableAndOccupied()
        {
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 3);
            var now = DateTime.UtcNow;

            lot.Park(new VehicleReg("A"), VehicleSize.SMALL, now);
            lot.Park(new VehicleReg("B"), VehicleSize.MEDIUM, now);

            var (available, occupied) = lot.Snapshot();

            available.Should().Be(1);
            occupied.Should().Be(2);
        }
    }
    }
