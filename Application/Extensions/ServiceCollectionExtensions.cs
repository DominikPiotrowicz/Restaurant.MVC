﻿using Application.Mappings;
using Application.RestaurantDto.Commands.CreateRestaurant;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class ServiceCollectionExtensions
	{
		public static void AddApplication(this IServiceCollection services)
		{
			services.AddMediatR(typeof(CreateRestaurantCommand));

			services.AddAutoMapper(typeof(RestaurantMappingProfile));

			services.AddValidatorsFromAssemblyContaining<CreateRestaurantCommandValidator>()
				.AddFluentValidationAutoValidation()
				.AddFluentValidationClientsideAdapters();
		}
	}
}
