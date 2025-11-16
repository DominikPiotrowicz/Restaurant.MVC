using MediatR;

namespace Application.RestaurantDto.Commands.CreateRestaurant
{
    public class CreateRestaurantCommand : RestaurantDto, IRequest
    {
        public string? OwnerId { get; set; }
    }
}
