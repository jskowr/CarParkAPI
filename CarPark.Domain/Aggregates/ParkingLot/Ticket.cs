using CarPark.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPark.Domain.Aggregates.ParkingLot
{
    public sealed class Ticket
    {
        public VehicleReg VehicleReg { get; }
        public VehicleSize VehicleType { get; }
        public SpaceNumber SpaceNumber { get; }
        public DateTime TimeInUtc { get; }
        public DateTime? TimeOutUtc { get; private set; }

        private Ticket(VehicleReg reg, VehicleSize type, SpaceNumber space, DateTime timeInUtc)
            => (VehicleReg, VehicleType, SpaceNumber, TimeInUtc) = (reg, type, space, timeInUtc);

        public static Ticket Start(VehicleReg reg, VehicleSize type, SpaceNumber space, DateTime utcNow)
            => new(reg, type, space, utcNow);

        public void Close(DateTime utcNow)
        {
            if (TimeOutUtc is not null) throw new ValidationException("Ticket already closed.");
            if (utcNow < TimeInUtc) throw new ValidationException("Exit time before entry time.");
            TimeOutUtc = utcNow;
        }
    }

}
