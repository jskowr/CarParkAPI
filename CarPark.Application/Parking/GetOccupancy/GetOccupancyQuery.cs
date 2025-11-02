using MediatR;
namespace CarPark.Application.Parking.GetOccupancy
{
    public sealed record GetOccupancyQuery() : IRequest<GetOccupancyResult>;
}
