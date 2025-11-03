using CarPark.Application.Parking.ExitVehicle;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Exceptions;
using CarPark.Domain.Services;
using CarPark.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Moq;
using Parking.Domain.Aggregates.ParkingLot;

namespace Tests.CarPark.Application
{
    public sealed class ExitVehicleCommandHandlerTests
    {
        [Theory]
        [InlineData(1, 0.10, VehicleSize.SMALL)]
        [InlineData(5, 2.00, VehicleSize.MEDIUM)]
        [InlineData(6, 3.40, VehicleSize.LARGE)]
        public async Task Handle_ShouldExitVehicle_AndReturnMappedResult(
                    int minutesParked, double expectedAmount, VehicleSize vehicleSize)
        {
            // Arrange
            var reg = "ABC123";
            var timeIn = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var timeOut = timeIn.AddMinutes(minutesParked);

            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 50);
            var ticket = lot.Park(new VehicleReg(reg), vehicleSize, timeIn);

            var repo = new Mock<IParkingLotRepository>();
            repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);
            repo.Setup(r => r.SaveAsync(lot, ticket, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var clock = new Mock<IClock>();
            clock.SetupGet(c => c.UtcNow).Returns(timeOut);

            var pricing = new Mock<IPricingPolicy>();
            pricing
                .Setup(p => p.Calculate(timeIn, timeOut, vehicleSize))
                .Returns(new Money((decimal)expectedAmount, "GBP"));

            var mediator = new Mock<IMediator>();
            mediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = new ExitVehicleCommandHandler(repo.Object, pricing.Object, clock.Object, mediator.Object);

            // Act
            var result = await sut.Handle(new ExitVehicleCommand(reg), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleReg.Should().Be(reg);
            Math.Round((decimal)result.VehicleCharge, 2).Should().Be(Math.Round((decimal)expectedAmount, 2));
            result.TimeInUtc.Should().Be(timeIn);
            result.TimeOutUtc.Should().Be(timeOut);

            repo.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
            repo.Verify(r => r.SaveAsync(lot, ticket, It.IsAny<CancellationToken>()), Times.Once);
            pricing.Verify(p => p.Calculate(timeIn, timeOut, vehicleSize), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenVehicleNotParked()
        {
            // Arrange
            var reg = "EEE-777";
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 10);

            var repo = new Mock<IParkingLotRepository>();
            repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);

            var clock = new Mock<IClock>();
            clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);

            var pricing = new Mock<IPricingPolicy>();
            var mediator = new Mock<IMediator>();

            var sut = new ExitVehicleCommandHandler(repo.Object, pricing.Object, clock.Object, mediator.Object);

            // Act
            var act = async () => await sut.Handle(new ExitVehicleCommand(reg), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<VehicleNotFoundException>();

            repo.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
            repo.Verify(r => r.SaveAsync(It.IsAny<ParkingLot>(), It.IsAny<Ticket>(), It.IsAny<CancellationToken>()), Times.Never);
            mediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
            pricing.VerifyNoOtherCalls();
        }
    }
}
