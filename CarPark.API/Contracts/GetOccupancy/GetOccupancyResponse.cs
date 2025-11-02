namespace CarPark.API.Contracts.GetOccupancy
{
    public sealed class GetOccupancyResponse
    {
        public int AvailableSpaces { get; init; }
        public int OccupiedSpaces { get; init; }
    }
}
