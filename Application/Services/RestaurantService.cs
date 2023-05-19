using AutoMapper;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMapper _mapper;

        public RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper)
        {
            _mapper = mapper;
            _restaurantRepository = restaurantRepository;
        }

        public async Task Create(RestaurantDto.RestaurantDto restaurantDto)
        {
           var restaurant = _mapper.Map < Domain.Entities.Restaurant>(restaurantDto);
            restaurant.EncodeName();

            await _restaurantRepository.Create(restaurant);
        }

		public async Task<IEnumerable<RestaurantDto.RestaurantDto>> GetAll()
		{
            var restaurats = await _restaurantRepository.GetAll();
            var dtos = _mapper.Map<IEnumerable<RestaurantDto.RestaurantDto>>(restaurats);

            return dtos;
		}
	}
}
