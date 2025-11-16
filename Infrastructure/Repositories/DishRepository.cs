using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
	public class DishRepository : IDishRepository
	{
		private readonly RestaurantDbContext _dbContext;

		public DishRepository(RestaurantDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task Create(Dish dish)
		{
			_dbContext.Add(dish);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<Dish?> GetById(int id)
			=> await _dbContext.Dishes.FirstOrDefaultAsync(d => d.Id == id);

		public async Task Delete(Dish dish)
		{
			_dbContext.Remove(dish);
			await _dbContext.SaveChangesAsync();
		}

		public Task Commit()
			=> _dbContext.SaveChangesAsync();
	}
}
