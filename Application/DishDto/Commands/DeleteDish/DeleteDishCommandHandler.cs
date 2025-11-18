using Domain.Interfaces;
using MediatR;

namespace Application.DishDto.Commands.DeleteDish
{
	public class DeleteDishCommandHandler : IRequestHandler<DeleteDishCommand>
	{
		private readonly IDishRepository _dishRepository;
		private readonly IAuditLogService _auditLogService;

		public DeleteDishCommandHandler(
			IDishRepository dishRepository,
			IAuditLogService auditLogService)
		{
			_dishRepository = dishRepository;
			_auditLogService = auditLogService;
		}

		public async Task<Unit> Handle(DeleteDishCommand request, CancellationToken cancellationToken)
		{
			var dish = await _dishRepository.GetById(request.Id);

			if (dish == null)
			{
				throw new InvalidOperationException($"Dish with id '{request.Id}' not found.");
			}

			if (dish.Restaurant?.OwnerId != request.CurrentUserId)
			{
				throw new UnauthorizedAccessException("Only the restaurant owner can delete dishes.");
			}

			var deletedDishInfo = new
			{
				Name = dish.Name,
				Category = dish.Category,
				Price = dish.Price,
				RestaurantId = dish.RestaurantId
			};

			await _dishRepository.Delete(dish);

			await _auditLogService.LogAsync(
				"Dish.Delete",
				"Dish",
				request.Id.ToString(),
				deletedDishInfo,
				null,
				true);

			return Unit.Value;
		}
	}
}
