using Application.Mappings;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRestaurantService, RestaurantService>();

            services.AddAutoMapper(typeof(RestaurantMappingProfile));
        }
    }
}
