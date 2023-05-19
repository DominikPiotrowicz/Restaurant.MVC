using Application.Mappings;
using Application.RestaurantDto;
using Application.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddApplication(this IServiceCollection services)
		{
			services.AddScoped<IRestaurantService, RestaurantService>();

			services.AddAutoMapper(typeof(RestaurantMappingProfile));

			services.AddValidatorsFromAssemblyContaining<RestaurantDtoValidator>()
				.AddFluentValidationAutoValidation()
				.AddFluentValidationClientsideAdapters();
		}
	}
}
