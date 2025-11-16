using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
	public class ApplicationUser : IdentityUser
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }

		// Navigation property
		public virtual ICollection<Restaurant> OwnedRestaurants { get; set; } = new List<Restaurant>();
	}
}
