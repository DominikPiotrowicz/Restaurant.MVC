using Domain.Interfaces;
using MediatR;

namespace Application.DishDto.Commands.EditDish
{
	public class EditDishCommandHandler : IRequestHandler<EditDishCommand>
	{
		private readonly IDishRepository _dishRepository;

		public EditDishCommandHandler(IDishRepository dishRepository)
		{
			_dishRepository = dishRepository;
		}

		public async Task<Unit> Handle(EditDishCommand request, CancellationToken cancellationToken)
		{
			var dish = await _dishRepository.GetById(request.Id);

			if (dish == null)
			{
				throw new InvalidOperationException($"Dish with id '{request.Id}' not found.");
			}

			dish.Name = request.Name;
			dish.Description = request.Description;
			dish.Price = request.Price;
			dish.Category = request.Category;

			await _dishRepository.Commit();

			return Unit.Value;
		}
	}
}
