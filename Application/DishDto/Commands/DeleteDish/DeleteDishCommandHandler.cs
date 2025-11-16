using Domain.Interfaces;
using MediatR;

namespace Application.DishDto.Commands.DeleteDish
{
	public class DeleteDishCommandHandler : IRequestHandler<DeleteDishCommand>
	{
		private readonly IDishRepository _dishRepository;

		public DeleteDishCommandHandler(IDishRepository dishRepository)
		{
			_dishRepository = dishRepository;
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

			await _dishRepository.Delete(dish);

			return Unit.Value;
		}
	}
}
