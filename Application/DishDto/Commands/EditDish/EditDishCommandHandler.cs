using Domain.Interfaces;
using MediatR;

namespace Application.DishDto.Commands.EditDish
{
	public class EditDishCommandHandler : IRequestHandler<EditDishCommand>
	{
		private readonly IDishRepository _dishRepository;
		private readonly IAuditLogService _auditLogService;

		public EditDishCommandHandler(
			IDishRepository dishRepository,
			IAuditLogService auditLogService)
		{
			_dishRepository = dishRepository;
			_auditLogService = auditLogService;
		}

		public async Task<Unit> Handle(EditDishCommand request, CancellationToken cancellationToken)
		{
			var dish = await _dishRepository.GetById(request.Id);

			if (dish == null)
			{
				throw new InvalidOperationException($"Dish with id '{request.Id}' not found.");
			}

			if (dish.Restaurant?.OwnerId != request.CurrentUserId)
			{
				throw new UnauthorizedAccessException("Only the restaurant owner can edit dishes.");
			}

			var oldValues = new
			{
				Name = dish.Name,
				Description = dish.Description,
				Price = dish.Price,
				Category = dish.Category
			};

			dish.Name = request.Name;
			dish.Description = request.Description;
			dish.Price = request.Price;
			dish.Category = request.Category;

			await _dishRepository.Commit();

			var newValues = new
			{
				Name = dish.Name,
				Description = dish.Description,
				Price = dish.Price,
				Category = dish.Category
			};

			await _auditLogService.LogAsync(
				"Dish.Edit",
				"Dish",
				dish.Id.ToString(),
				oldValues,
				newValues,
				true);

			return Unit.Value;
		}
	}
}
