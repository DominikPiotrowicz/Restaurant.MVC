using FluentValidation;

namespace Application.RestaurantDto
{
	public class RestaurantDtoValidator : AbstractValidator<RestaurantDto>
	{
        public RestaurantDtoValidator()
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
