using MediatR;

namespace Application.DishDto.Commands.CreateDish
{
	public class CreateDishCommand : IRequest
	{
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public decimal Price { get; set; }
		public string RestaurantEncodedName { get; set; } = default!;
	}
}
