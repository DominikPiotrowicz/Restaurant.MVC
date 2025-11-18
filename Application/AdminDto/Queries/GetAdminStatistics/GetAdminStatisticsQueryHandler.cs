using Domain.Entities;
using Infrastructure.Persistance;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminDto.Queries.GetAdminStatistics
{
	public class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
	{
		private readonly RestaurantDbContext _dbContext;
		private readonly UserManager<ApplicationUser> _userManager;

		public GetAdminStatisticsQueryHandler(RestaurantDbContext dbContext, UserManager<ApplicationUser> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}

		public async Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken cancellationToken)
		{
			var totalRestaurants = await _dbContext.Restaurants.CountAsync(cancellationToken);
			var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
			var totalDishes = await _dbContext.Dishes.CountAsync(cancellationToken);
			var restaurantsWithDelivery = await _dbContext.Restaurants.CountAsync(r => r.HasDelivery, cancellationToken);

			var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			var restaurantsCreatedThisMonth = await _dbContext.Restaurants
				.CountAsync(r => r.CreatedAt >= firstDayOfMonth, cancellationToken);

			return new AdminStatisticsDto
			{
				TotalRestaurants = totalRestaurants,
				TotalUsers = totalUsers,
				TotalDishes = totalDishes,
				RestaurantsWithDelivery = restaurantsWithDelivery,
				RestaurantsCreatedThisMonth = restaurantsCreatedThisMonth
			};
		}
	}
}
