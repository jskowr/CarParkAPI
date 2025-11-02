using CarPark.Domain.Abstractions;

namespace CarPark.Domain.Exceptions
{
    public sealed class NoSpacesAvailableException : DomainException
    {
        public NoSpacesAvailableException()
            : base("There are no free parking spaces available.") { }
    }
}
