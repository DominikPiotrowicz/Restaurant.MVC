using Domain.Entities;

namespace Domain.Interfaces
{
	public  interface IRestaurantRepository
    {
        Task Create(Restaurant restaurant);
        Task<Restaurant> GetByName(string name);
        Task<IEnumerable<Restaurant>> GetAll();
        Task<Restaurant> GetByEncodedName(string endodedName);
        Task Commit();
    }
}
