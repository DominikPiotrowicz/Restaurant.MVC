using AutoMapper;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.RestaurantDto.Queries.SearchRestaurants
{
	public class SearchRestaurantsQueryHandler : IRequestHandler<SearchRestaurantsQuery, IEnumerable<RestaurantDto>>
	{
		private readonly IRestaurantRepository _restaurantRepository;
		private readonly IMapper _mapper;

		public SearchRestaurantsQueryHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
		{
			_restaurantRepository = restaurantRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<RestaurantDto>> Handle(SearchRestaurantsQuery request, CancellationToken cancellationToken)
		{
			var restaurants = await _restaurantRepository.GetAll();

			// Filtrowanie po frazie wyszukiwania (nazwa lub opis)
			if (!string.IsNullOrWhiteSpace(request.SearchPhrase))
			{
				var searchPhrase = request.SearchPhrase.ToLower();
				restaurants = restaurants.Where(r =>
					r.Name.ToLower().Contains(searchPhrase) ||
					(r.Description != null && r.Description.ToLower().Contains(searchPhrase)));
			}

			// Filtrowanie po kategorii
			if (!string.IsNullOrWhiteSpace(request.Category))
			{
				restaurants = restaurants.Where(r => r.Category == request.Category);
			}

			// Filtrowanie po dostawie
			if (request.HasDelivery.HasValue)
			{
				restaurants = restaurants.Where(r => r.HasDelivery == request.HasDelivery.Value);
			}

			var dtos = _mapper.Map<IEnumerable<RestaurantDto>>(restaurants);

			return dtos;
		}
	}
}
