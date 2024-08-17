using Domain.Interfaces;
using MediatR;

namespace Application.RestaurantDto.Commands.EditRestaurant
{
	public class EditRestaurantCommandHandler : IRequestHandler<EditRestaurantCommand>
	{
		private readonly IRestaurantRepository _repository;

		public EditRestaurantCommandHandler(IRestaurantRepository repository)
        {
			_repository = repository;
		}

        public async Task<Unit> Handle(EditRestaurantCommand request, CancellationToken cancellationToken)
		{
			var restaurant = await _repository.GetByEncodedName(request.EncodedName!);

			restaurant.Name = request.Name;
			restaurant.Description = request.Description;
			restaurant.Category = request.Category;
			restaurant.HasDelivery = request.HasDelivery;
			restaurant.ContactEmail = request.ContactEmail;
			restaurant.ContactNumber = request.ContactNumber;
			restaurant.Address.City = request.City;
			restaurant.Address.Street = request.Street;
			restaurant.Address.PostalCode = request.PostalCode;

			await _repository.Commit();

			return Unit.Value;
		}
	}
}
