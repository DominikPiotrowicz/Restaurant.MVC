using System.ComponentModel.DataAnnotations;

namespace Restaurant.MVC.Models.Account
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Email jest wymagany")]
		[EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
		public string Email { get; set; } = default!;

		[Required(ErrorMessage = "Hasło jest wymagane")]
		[DataType(DataType.Password)]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć od 6 do 100 znaków")]
		public string Password { get; set; } = default!;

		[DataType(DataType.Password)]
		[Display(Name = "Potwierdź hasło")]
		[Compare("Password", ErrorMessage = "Hasła nie są identyczne")]
		public string ConfirmPassword { get; set; } = default!;

		public string? FirstName { get; set; }
		public string? LastName { get; set; }
	}
}
