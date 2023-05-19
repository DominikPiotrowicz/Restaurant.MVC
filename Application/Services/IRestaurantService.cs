using Domain.Entities;
using Application.RestaurantDto;

namespace Application.Services
{
    public interface IRestaurantService
    {
        Task Create(RestaurantDto.RestaurantDto restaurant);
        Task<IEnumerable<RestaurantDto.RestaurantDto>> GetAll();
    }
}