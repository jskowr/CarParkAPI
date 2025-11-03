using CarPark.Application.Parking.ParkVehicle;
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
    public sealed class ParkVehicleCommandHandlerTests
    {
        [Theory]
        [InlineData("SMALL", VehicleSize.SMALL, 10, 1)]
        [InlineData("medium", VehicleSize.MEDIUM, 10, 1)]
        [InlineData("Large", VehicleSize.LARGE, 3, 1)]
        public async Task Handle_ShouldParkVehicle_AndReturnMappedResult(
            string vehicleSizeString, VehicleSize expectedVehicleSize, int capacity, int expectedSpace)
        {
            // Arrange
            var reg = "ABC123";
            var fixedNow = new DateTime(2025, 01, 01, 12, 34, 56, DateTimeKind.Utc);

            var lot = ParkingLot.Create(Guid.NewGuid(), capacity);
            var ticket = Ticket.Start(new VehicleReg(reg), expectedVehicleSize, lot.Spaces.OrderBy(s => s.Number.Value).First(), fixedNow);

            var repo = new Mock<IParkingLotRepository>();
            repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);
            repo.Setup(r => r.SaveAsync(lot, ticket, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var clock = new Mock<IClock>();
            clock.SetupGet(c => c.UtcNow).Returns(fixedNow);

            var mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var sut = new ParkVehicleCommandHandler(repo.Object, clock.Object, mediator.Object);

            // Act
            var result = await sut.Handle(new ParkVehicleCommand(reg, vehicleSizeString), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.VehicleReg.Should().Be(reg);
            result.SpaceNumber.Should().Be(expectedSpace);
            result.TimeInUtc.Should().Be(fixedNow);

            lot.OccupiedSpaces.Should().Be(1);
            lot.AvailableSpaces.Should().Be(capacity - 1);

            repo.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
            repo.Verify(r => r.SaveAsync(
                It.IsAny<ParkingLot>(),
                It.Is<Ticket>(t =>
                    t.VehicleReg.Value == reg &&
                    t.VehicleSize == expectedVehicleSize &&
                    t.Space.Number.Value == expectedSpace &&
                    t.TimeInUtc == fixedNow),
                It.IsAny<CancellationToken>()),
            Times.Once);

            await FluentActions.Invoking(async () =>
            {
                lot.Park(new VehicleReg(reg), expectedVehicleSize, fixedNow);
                await Task.CompletedTask;
            }).Should().ThrowAsync<VehicleAlreadyParkedException>();
        }

        [Fact]
        public async Task Handle_ShouldThrow_ForUnknownVehicleSize()
        {
            // Arrange
            var reg = "XYZ999";
            var lot = ParkingLot.Create(Guid.NewGuid(), 5);

            var repo = new Mock<IParkingLotRepository>();
            repo.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);

            var clock = new Mock<IClock>();
            clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);

            var mediator = new Mock<IMediator>();

            var sut = new ParkVehicleCommandHandler(repo.Object, clock.Object, mediator.Object);

            // Act
            var act = async () => await sut.Handle(new ParkVehicleCommand(reg, "UNKNOWN_SIZE"), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Unknown VehicleType*");

            repo.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Never);
            repo.Verify(r => r.SaveAsync(It.IsAny<ParkingLot>(), It.IsAny<Ticket>(), It.IsAny<CancellationToken>()), Times.Never);
            mediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}