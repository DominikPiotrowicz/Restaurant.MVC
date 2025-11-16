using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.RestaurantDto.Queries.GetRestaurantByEncodedName
{
	public class GetRestaurantByEncodedNameQueryHandler : IRequestHandler<GetRestaurantByEncodedNameQuery, RestaurantDto>
	{
		private readonly IRestaurantRepository _restaurantRepository;
		private readonly IMapper _mapper;

		public GetRestaurantByEncodedNameQueryHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
        {
			_restaurantRepository = restaurantRepository;
			_mapper = mapper;
		}

        public async Task<RestaurantDto> Handle(GetRestaurantByEncodedNameQuery request, CancellationToken cancellationToken)
		{
			var restaurant = await _restaurantRepository.GetByEncodedName(request.EncodedName);

			var dto = _mapper.Map<RestaurantDto>(restaurant);

			return dto;
		}
	}
}
