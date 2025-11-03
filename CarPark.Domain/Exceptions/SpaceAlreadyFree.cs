using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Exceptions
{
    public sealed class SpaceAlreadyFree : DomainException
    {
        public SpaceAlreadyFree(int spaceNumber)
            : base($"Space {spaceNumber} is already free.")
        {

        }
    }
}
