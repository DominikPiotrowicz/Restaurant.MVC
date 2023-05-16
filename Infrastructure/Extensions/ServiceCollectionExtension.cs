using Domain.Interfaces;
using Infrastructure.Persistance;
using Infrastructure.Repositories;
using Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RestaurantDbContext>(option => option.UseSqlServer(
            configuration.GetConnectionString("Restaruant")));

            services.AddScoped<RestaurantSeeder>();
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        }
    }
}
