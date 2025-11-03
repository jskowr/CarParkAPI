using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Exceptions
{
    public sealed class SpaceAlreadyOccupied : DomainException {
        public SpaceAlreadyOccupied(int spaceNumber)
            : base($"Space {spaceNumber} is already occupied.") {
        
        }
    }
}
