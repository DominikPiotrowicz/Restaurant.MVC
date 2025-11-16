using Domain.Entities;

namespace Domain.Interfaces
{
	public interface IDishRepository
	{
		Task Create(Dish dish);
		Task<Dish?> GetById(int id);
		Task Delete(Dish dish);
		Task Commit();
	}
}
