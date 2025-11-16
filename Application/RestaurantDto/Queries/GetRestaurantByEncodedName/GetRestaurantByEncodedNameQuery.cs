using MediatR;

namespace Application.RestaurantDto.Queries.GetRestaurantByEncodedName
{
	public class GetRestaurantByEncodedNameQuery : IRequest<RestaurantDto>
	{
		public string EncodedName { get; set; }

        public GetRestaurantByEncodedNameQuery(string encodedName)
        {
                EncodedName = encodedName;
        }
    }
}
