using Domain.Entities;
using Infrastructure.Persistance;

namespace Infrastructure.Seeders
{
    public class RestaurantSeeder
    {
        private readonly RestaurantDbContext _dbContext;

        public RestaurantSeeder(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Seed()
        {
            if (await _dbContext.Database.CanConnectAsync())
            {
                if (!_dbContext.Restaurants.Any())
                {
                    var restaurants = GetRestaurants();
                    _dbContext.Restaurants.AddRange(restaurants);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        public static IEnumerable<Restaurant> GetRestaurants()
        {
            var restaurants = new List<Restaurant>()
            {
                new Restaurant()
                {
                    Name = "KFC",
                    Category = "Fast Food",
                    Description = " KFC Kentucky Fried Chicken) is an American fast food restaurant chain headquartered in Louisville, Kentucky",
                    ContactEmail = "contact@kfc.com",
                    HasDelivery = true,
                    ContactNumber = "123454578",
                    Dishes = new List<Dish>()
                    {
                        new Dish()
                        {
                            Name = "Nashville Hot Chicken",
                            Price = 10.30M,
                            Description = "Nashville Hot Chicken"
                        },
                        new Dish()
                        {
                            Name = "Chicken Nuggets",
                            Price = 5.30M,
                            Description = "Chicken Nuggets"
                        },
                    },
                    Address = new Address()
                    {
                        City = "Kraków",
                        Street = "Długa 50",
                        PostalCode = "30-001"
                    }
                },
                new Restaurant()
                {
                    Name = "McDonald Szewska",
                    Category = "Fast Food",
                    Description =
                        "McDonald's Corporation (McDonald's), incorporated on December 21, 1964, operates and franchises McDonald's restaurants.",
                    ContactEmail = "contact@mcdonald.com",
                    ContactNumber = "123454578",
                    HasDelivery = true,
                    Address = new Address()
                    {
                        City = "Kraków",
                        Street = "Szewska 2",
                        PostalCode = "30-001"
                    }
                 }
            };

            foreach (var restaurant in restaurants)
            {
                restaurant.EncodeName();
            }

            return restaurants;
        }

    }
}
