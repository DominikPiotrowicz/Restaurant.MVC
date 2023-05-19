using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
	public class RestaurantRepository : IRestaurantRepository
	{
		private readonly RestaurantDbContext _dbContext;

		public RestaurantRepository(RestaurantDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task Create(Restaurant restaurant)
		{
			_dbContext.Add(restaurant);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<IEnumerable<Restaurant>> GetAll()
			=> await _dbContext.Restaurants.Include(r => r.Address).ToListAsync();
	}
}
