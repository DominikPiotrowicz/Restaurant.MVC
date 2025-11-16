using MediatR;

namespace Application.DishDto.Commands.DeleteDish
{
	public class DeleteDishCommand : IRequest
	{
		public int Id { get; set; }
		public string RestaurantEncodedName { get; set; } = default!;
		public string? CurrentUserId { get; set; }

		public DeleteDishCommand(int id, string restaurantEncodedName, string? currentUserId = null)
		{
			Id = id;
			RestaurantEncodedName = restaurantEncodedName;
			CurrentUserId = currentUserId;
		}
	}
}
