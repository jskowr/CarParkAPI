using CarPark.Application.Parking.GetOccupancy;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Parking.Domain.Aggregates.ParkingLot;

namespace Tests.CarPark.Application
{
    public sealed class GetOccupancyHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnCorrectOccupancy_ForParkingLot()
        {
            // Arrange
            var lot = ParkingLot.Create(Guid.NewGuid(), capacity: 5);

            lot.Park(new VehicleReg("AAA123"), VehicleSize.SMALL, DateTime.UtcNow);
            lot.Park(new VehicleReg("BBB456"), VehicleSize.MEDIUM, DateTime.UtcNow);

            var repoMock = new Mock<IParkingLotRepository>();
            repoMock
                .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lot);

            var handler = new GetOccupancyHandler(repoMock.Object);

            // Act
            var result = await handler.Handle(new GetOccupancyQuery(), CancellationToken.None);

            // Assert
            result.AvailableSpaces.Should().Be(3);
            result.OccupiedSpaces.Should().Be(2);

            repoMock.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
