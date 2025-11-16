using FluentValidation;

namespace Application.DishDto.Commands.CreateDish
{
	public class CreateDishCommandValidator : AbstractValidator<CreateDishCommand>
	{
		public CreateDishCommandValidator()
		{
			RuleFor(d => d.Name)
				.NotEmpty()
				.WithMessage("Nazwa dania jest wymagana")
				.MinimumLength(2)
				.WithMessage("Nazwa dania musi mieć co najmniej 2 znaki")
				.MaximumLength(50)
				.WithMessage("Nazwa dania może mieć maksymalnie 50 znaków");

			RuleFor(d => d.Description)
				.NotEmpty()
				.WithMessage("Opis dania jest wymagany")
				.MaximumLength(200)
				.WithMessage("Opis może mieć maksymalnie 200 znaków");

			RuleFor(d => d.Price)
				.GreaterThan(0)
				.WithMessage("Cena musi być większa niż 0")
				.LessThanOrEqualTo(1000)
				.WithMessage("Cena musi być mniejsza niż 1000");

			RuleFor(d => d.RestaurantEncodedName)
				.NotEmpty()
				.WithMessage("Nazwa restauracji jest wymagana");
		}
	}
}
