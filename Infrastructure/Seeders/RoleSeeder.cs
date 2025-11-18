using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeders
{
	public class RoleSeeder
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<ApplicationUser> _userManager;

		public RoleSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}

		public async Task Seed()
		{
			// Tworzenie roli Administrator
			if (!await _roleManager.RoleExistsAsync("Administrator"))
			{
				await _roleManager.CreateAsync(new IdentityRole("Administrator"));
			}

			// Tworzenie roli User (zwykły użytkownik)
			if (!await _roleManager.RoleExistsAsync("User"))
			{
				await _roleManager.CreateAsync(new IdentityRole("User"));
			}

			// Tworzenie domyślnego administratora
			var adminEmail = "admin@restaurant.com";
			var adminUser = await _userManager.FindByEmailAsync(adminEmail);

			if (adminUser == null)
			{
				adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					EmailConfirmed = true,
					FirstName = "Administrator",
					LastName = "System"
				};

				var result = await _userManager.CreateAsync(adminUser, "Admin123!");

				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(adminUser, "Administrator");
				}
			}
		}
	}
}
