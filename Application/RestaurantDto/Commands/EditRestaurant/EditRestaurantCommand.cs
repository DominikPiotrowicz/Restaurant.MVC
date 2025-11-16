using MediatR;

namespace Application.RestaurantDto.Commands.EditRestaurant
{
	public class EditRestaurantCommand : RestaurantDto, IRequest
	{
		public string? CurrentUserId { get; set; }
	}
}
