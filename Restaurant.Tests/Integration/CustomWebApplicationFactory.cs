using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Restaurant.Tests.Integration
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				// Remove the existing DbContext registration
				var descriptor = services.SingleOrDefault(
					d => d.ServiceType == typeof(DbContextOptions<RestaurantDbContext>));

				if (descriptor != null)
				{
					services.Remove(descriptor);
				}

				// Add DbContext using in-memory database for testing
				services.AddDbContext<RestaurantDbContext>(options =>
				{
					options.UseInMemoryDatabase("InMemoryTestDb");
				});

				// Build the service provider
				var sp = services.BuildServiceProvider();

				// Create a scope to obtain a reference to the database context
				using (var scope = sp.CreateScope())
				{
					var scopedServices = scope.ServiceProvider;
					var db = scopedServices.GetRequiredService<RestaurantDbContext>();
					var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
					var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

					// Ensure the database is created
					db.Database.EnsureCreated();

					// Seed test data
					SeedTestData(db, userManager, roleManager).Wait();
				}
			});
		}

		private async Task SeedTestData(
			RestaurantDbContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager)
		{
			// Create roles
			if (!await roleManager.RoleExistsAsync("Administrator"))
			{
				await roleManager.CreateAsync(new IdentityRole("Administrator"));
			}

			if (!await roleManager.RoleExistsAsync("User"))
			{
				await roleManager.CreateAsync(new IdentityRole("User"));
			}

			// Create test admin user
			var adminUser = await userManager.FindByEmailAsync("admin@test.com");
			if (adminUser == null)
			{
				adminUser = new ApplicationUser
				{
					UserName = "admin@test.com",
					Email = "admin@test.com",
					EmailConfirmed = true,
					FirstName = "Admin",
					LastName = "Test"
				};
				await userManager.CreateAsync(adminUser, "Admin123!");
				await userManager.AddToRoleAsync(adminUser, "Administrator");
			}

			// Create test regular user
			var regularUser = await userManager.FindByEmailAsync("user@test.com");
			if (regularUser == null)
			{
				regularUser = new ApplicationUser
				{
					UserName = "user@test.com",
					Email = "user@test.com",
					EmailConfirmed = true,
					FirstName = "Test",
					LastName = "User"
				};
				await userManager.CreateAsync(regularUser, "User123!");
				await userManager.AddToRoleAsync(regularUser, "User");
			}

			// Create test restaurant
			if (!context.Restaurants.Any())
			{
				var restaurant = new Domain.Entities.Restaurant
				{
					Name = "Test Restaurant",
					Description = "A test restaurant",
					Category = "Italian",
					HasDelivery = true,
					ContactEmail = "test@restaurant.com",
					ContactNumber = "123456789",
					OwnerId = regularUser.Id,
					CreatedAt = DateTime.UtcNow,
					Address = new Address
					{
						City = "Warsaw",
						Street = "Test Street",
						PostalCode = "00-001"
					}
				};
				restaurant.EncodeName();

				context.Restaurants.Add(restaurant);
				await context.SaveChangesAsync();

				// Add test dish
				context.Dishes.Add(new Dish
				{
					Name = "Test Dish",
					Description = "A test dish",
					Price = 25.99m,
					Category = Domain.Enums.DishCategory.MainCourse,
					RestaurantId = restaurant.Id
				});
				await context.SaveChangesAsync();
			}
		}
	}
}
