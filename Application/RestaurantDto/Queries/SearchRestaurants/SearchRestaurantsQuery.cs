using MediatR;

namespace Application.RestaurantDto.Queries.SearchRestaurants
{
	public class SearchRestaurantsQuery : IRequest<IEnumerable<RestaurantDto>>
	{
		public string? SearchPhrase { get; set; }
		public string? Category { get; set; }
		public bool? HasDelivery { get; set; }
	}
}
