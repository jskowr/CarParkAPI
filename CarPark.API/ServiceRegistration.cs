using CarPark.Application.Parking.GetOccupancy;
using CarPark.Application.Parking.ParkVehicle;
using CarPark.Domain.Aggregates.ParkingLot;
using CarPark.Domain.Services;
using CarPark.Infrastructure;
using CarPark.Infrastructure.Repositories;

namespace CarPark.API
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ParkVehicleCommand).Assembly));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetOccupancyQuery).Assembly));


            services.AddPricingService(config);

            services.AddSingleton<IParkingLotRepository, ParkingLotRepository>();
            services.AddSingleton<IClock, SystemClock>();

            return services;
        }

        private static IServiceCollection AddPricingService(this IServiceCollection services, IConfiguration config)
        {
            var section = config.GetSection("Pricing");

            var table = new PricingTable(
                SmallPerMinute: section.GetValue<decimal>("SmallPerMinute"),
                MediumPerMinute: section.GetValue<decimal>("MediumPerMinute"),
                LargePerMinute: section.GetValue<decimal>("LargePerMinute"),
                SurchargeBlockMinutes: section.GetValue<int>("SurchargeBlockMinutes"),
                SurchargePerBlock: section.GetValue<decimal>("SurchargePerBlock")
            );

            services.AddSingleton<IPricingPolicy>(new PricingPolicy(table));

            return services;
        }
    }
}
