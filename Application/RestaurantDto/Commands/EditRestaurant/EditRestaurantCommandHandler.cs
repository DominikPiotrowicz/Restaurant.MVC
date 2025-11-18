using Domain.Interfaces;
using MediatR;

namespace Application.RestaurantDto.Commands.EditRestaurant
{
	public class EditRestaurantCommandHandler : IRequestHandler<EditRestaurantCommand>
	{
		private readonly IRestaurantRepository _repository;
		private readonly IAuditLogService _auditLogService;

		public EditRestaurantCommandHandler(
			IRestaurantRepository repository,
			IAuditLogService auditLogService)
        {
			_repository = repository;
			_auditLogService = auditLogService;
		}

        public async Task<Unit> Handle(EditRestaurantCommand request, CancellationToken cancellationToken)
		{
			var restaurant = await _repository.GetByEncodedName(request.EncodedName!);

			if (restaurant == null)
			{
				throw new InvalidOperationException($"Restaurant with encoded name '{request.EncodedName}' not found.");
			}

			if (restaurant.OwnerId != request.CurrentUserId)
			{
				throw new UnauthorizedAccessException("Only the restaurant owner can edit this restaurant.");
			}

			var oldValues = new
			{
				Name = restaurant.Name,
				Description = restaurant.Description,
				Category = restaurant.Category,
				HasDelivery = restaurant.HasDelivery,
				ContactEmail = restaurant.ContactEmail,
				ContactNumber = restaurant.ContactNumber,
				City = restaurant.Address.City,
				Street = restaurant.Address.Street,
				PostalCode = restaurant.Address.PostalCode
			};

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

			var newValues = new
			{
				Name = restaurant.Name,
				Description = restaurant.Description,
				Category = restaurant.Category,
				HasDelivery = restaurant.HasDelivery,
				ContactEmail = restaurant.ContactEmail,
				ContactNumber = restaurant.ContactNumber,
				City = restaurant.Address.City,
				Street = restaurant.Address.Street,
				PostalCode = restaurant.Address.PostalCode
			};

			await _auditLogService.LogAsync(
				"Restaurant.Edit",
				"Restaurant",
				restaurant.Id.ToString(),
				oldValues,
				newValues,
				true);

			return Unit.Value;
		}
	}
}
