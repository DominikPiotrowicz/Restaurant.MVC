using MediatR;

namespace Application.DishDto.Commands.DeleteDish
{
	public class DeleteDishCommand : IRequest
	{
		public int Id { get; set; }
		public string RestaurantEncodedName { get; set; } = default!;

		public DeleteDishCommand(int id, string restaurantEncodedName)
		{
			Id = id;
			RestaurantEncodedName = restaurantEncodedName;
		}
	}
}
