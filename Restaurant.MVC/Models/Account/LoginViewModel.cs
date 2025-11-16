using System.ComponentModel.DataAnnotations;

namespace Restaurant.MVC.Models.Account
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Email jest wymagany")]
		[EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
		public string Email { get; set; } = default!;

		[Required(ErrorMessage = "Hasło jest wymagane")]
		[DataType(DataType.Password)]
		public string Password { get; set; } = default!;

		[Display(Name = "Zapamiętaj mnie")]
		public bool RememberMe { get; set; }

		public string? ReturnUrl { get; set; }
	}
}
