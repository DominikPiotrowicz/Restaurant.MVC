using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.RestaurantDto.Queries.GetAllRestaurants
{
    public class GetAllRestaurantsQueryHandler : IRequestHandler<GetAllRestaurantsQuery, IEnumerable<RestaurantDto>>
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMapper _mapper;

        public GetAllRestaurantsQueryHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
        {
            _restaurantRepository = restaurantRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<RestaurantDto>> Handle(GetAllRestaurantsQuery request, CancellationToken cancellationToken)
        {
            var restaurats = await _restaurantRepository.GetAll();
            var dtos = _mapper.Map<IEnumerable<RestaurantDto>>(restaurats);

            return dtos;
        }
    }
}
