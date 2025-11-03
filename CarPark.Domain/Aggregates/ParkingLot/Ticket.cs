using CarPark.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace CarPark.Domain.Aggregates.ParkingLot
{
    public sealed class Ticket
    {
        public VehicleReg VehicleReg { get; }
        public VehicleSize VehicleSize { get; }
        public Space Space { get; }
        public DateTime TimeInUtc { get; }
        public DateTime? TimeOutUtc { get; private set; }

        private Ticket(VehicleReg reg, VehicleSize type, Space space, DateTime inUtc, DateTime? outUtc)
        {
            VehicleReg = reg;
            VehicleSize = type;
            Space = space;
            TimeInUtc = inUtc;
            TimeOutUtc = outUtc;
        }

        public static Ticket Start(VehicleReg reg, VehicleSize type, Space space, DateTime utcNow)
            => new(reg, type, space, utcNow, null);

        public static Ticket Rehydrate(VehicleReg reg, VehicleSize type, Space space, DateTime inUtc, DateTime? outUtc)
            => new(reg, type, space, inUtc, outUtc);

        public void Close(DateTime utcNow) => TimeOutUtc = utcNow;
    }

}
