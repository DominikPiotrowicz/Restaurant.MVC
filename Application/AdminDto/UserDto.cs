namespace Application.AdminDto
{
	public class UserDto
	{
		public string Id { get; set; } = default!;
		public string Email { get; set; } = default!;
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public bool EmailConfirmed { get; set; }
		public IList<string> Roles { get; set; } = new List<string>();
		public int RestaurantsCount { get; set; }
	}
}
