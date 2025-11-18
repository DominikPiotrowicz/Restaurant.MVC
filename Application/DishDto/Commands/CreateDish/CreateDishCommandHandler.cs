using Domain.Interfaces;
using MediatR;

namespace Application.DishDto.Commands.CreateDish
{
	public class CreateDishCommandHandler : IRequestHandler<CreateDishCommand>
	{
		private readonly IDishRepository _dishRepository;
		private readonly IRestaurantRepository _restaurantRepository;
		private readonly IAuditLogService _auditLogService;

		public CreateDishCommandHandler(
			IDishRepository dishRepository,
			IRestaurantRepository restaurantRepository,
			IAuditLogService auditLogService)
		{
			_dishRepository = dishRepository;
			_restaurantRepository = restaurantRepository;
			_auditLogService = auditLogService;
		}

		public async Task<Unit> Handle(CreateDishCommand request, CancellationToken cancellationToken)
		{
			var restaurant = await _restaurantRepository.GetByEncodedName(request.RestaurantEncodedName);

			if (restaurant == null)
			{
				throw new InvalidOperationException($"Restaurant with encoded name '{request.RestaurantEncodedName}' not found.");
			}

			if (restaurant.OwnerId != request.CurrentUserId)
			{
				throw new UnauthorizedAccessException("Only the restaurant owner can add dishes.");
			}

			var dish = new Domain.Entities.Dish
			{
				Name = request.Name,
				Description = request.Description,
				Price = request.Price,
				Category = request.Category,
				RestaurantId = restaurant.Id
			};

			await _dishRepository.Create(dish);

			await _auditLogService.LogAsync(
				"Dish.Create",
				"Dish",
				dish.Id.ToString(),
				null,
				new { Name = dish.Name, Category = dish.Category, Price = dish.Price, RestaurantId = restaurant.Id },
				true);

			return Unit.Value;
		}
	}
}
