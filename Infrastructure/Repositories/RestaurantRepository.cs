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

		public Task Commit()
			=> _dbContext.SaveChangesAsync();

		public async Task Create(Domain.Entities.Restaurant restaurant)
		{
			_dbContext.Add(restaurant);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<IEnumerable<Domain.Entities.Restaurant>> GetAll()
			=> await _dbContext.Restaurants.Include(r => r.Address).Include(r => r.Dishes).ToListAsync();

		public async Task<Restaurant?> GetByEncodedName(string encodedName)
			=> await _dbContext.Restaurants.Include(r => r.Address).Include(r => r.Dishes).FirstOrDefaultAsync(c=> c.EncodedName == encodedName);
			

        public Task<Restaurant?> GetByName(string name)
        => _dbContext.Restaurants.FirstOrDefaultAsync(cw => cw.Name.ToLower() == name.ToLower());
    }
}
