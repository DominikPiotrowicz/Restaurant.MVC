using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
