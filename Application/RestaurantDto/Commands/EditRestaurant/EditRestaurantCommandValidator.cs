using FluentValidation;

namespace Application.RestaurantDto.Commands.EditRestaurant
{
	public class EditRestaurantCommandValidator : AbstractValidator<EditRestaurantCommand>
	{
		public EditRestaurantCommandValidator()
		{
			RuleFor(c => c.Name)
				.NotEmpty()
				.MinimumLength(2)
				.MaximumLength(20);

			RuleFor(c => c.Description)
				.NotEmpty();

			RuleFor(c => c.ContactNumber)
				.MinimumLength(8)
				.MaximumLength(12);

			RuleFor(c => c.ContactEmail)
				.EmailAddress();
		}
	}
}
