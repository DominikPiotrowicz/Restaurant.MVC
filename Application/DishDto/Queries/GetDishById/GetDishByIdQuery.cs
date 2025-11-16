using Application.RestaurantDto;
using MediatR;

namespace Application.DishDto.Queries.GetDishById
{
	public class GetDishByIdQuery : IRequest<DishDto?>
	{
		public int Id { get; set; }

		public GetDishByIdQuery(int id)
		{
			Id = id;
		}
	}
}
