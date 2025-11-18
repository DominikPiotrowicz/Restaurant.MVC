using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistance;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Restaurant.Tests.Infrastructure.Repositories
{
	public class DishRepositoryTests : IDisposable
	{
		private readonly RestaurantDbContext _dbContext;
		private readonly DishRepository _repository;

		public DishRepositoryTests()
		{
			var options = new DbContextOptionsBuilder<RestaurantDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_dbContext = new RestaurantDbContext(options);
			_repository = new DishRepository(_dbContext);
		}

		[Fact]
		public async Task Create_ValidDish_AddsToDatabase()
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dish = new Dish
			{
				Name = "Pizza Margherita",
				Description = "Classic Italian pizza",
				Price = 25.99m,
				Category = DishCategory.Pizza,
				RestaurantId = restaurant.Id
			};

			// Act
			await _repository.Create(dish);

			// Assert
			var savedDish = await _dbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dish.Id);
			savedDish.Should().NotBeNull();
			savedDish!.Name.Should().Be(dish.Name);
			savedDish.Price.Should().Be(dish.Price);
			savedDish.Category.Should().Be(DishCategory.Pizza);
		}

		[Fact]
		public async Task GetById_ExistingDish_ReturnsDish()
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dish = new Dish
			{
				Name = "Pasta Carbonara",
				Description = "Creamy pasta",
				Price = 30.00m,
				Category = DishCategory.Pasta,
				RestaurantId = restaurant.Id
			};
			await _repository.Create(dish);

			// Act
			var result = await _repository.GetById(dish.Id);

			// Assert
			result.Should().NotBeNull();
			result!.Name.Should().Be(dish.Name);
			result.Price.Should().Be(dish.Price);
		}

		[Fact]
		public async Task GetById_NonExistingDish_ReturnsNull()
		{
			// Arrange
			var nonExistingId = 9999;

			// Act
			var result = await _repository.GetById(nonExistingId);

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task GetById_IncludesRestaurant()
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dish = new Dish
			{
				Name = "Test Dish",
				Description = "Test",
				Price = 10.00m,
				Category = DishCategory.MainCourse,
				RestaurantId = restaurant.Id
			};
			await _repository.Create(dish);

			// Act
			var result = await _repository.GetById(dish.Id);

			// Assert
			result.Should().NotBeNull();
			result!.Restaurant.Should().NotBeNull();
			result.Restaurant.Name.Should().Be(restaurant.Name);
		}

		[Fact]
		public async Task Delete_ExistingDish_RemovesFromDatabase()
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dish = new Dish
			{
				Name = "To Be Deleted",
				Description = "Test",
				Price = 15.00m,
				Category = DishCategory.Dessert,
				RestaurantId = restaurant.Id
			};
			await _repository.Create(dish);
			var dishId = dish.Id;

			// Act
			await _repository.Delete(dish);

			// Assert
			var deleted = await _dbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dishId);
			deleted.Should().BeNull();
		}

		[Fact]
		public async Task Commit_UpdatesDish_SavesChanges()
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dish = new Dish
			{
				Name = "Original Name",
				Description = "Original Description",
				Price = 20.00m,
				Category = DishCategory.Appetizer,
				RestaurantId = restaurant.Id
			};
			await _repository.Create(dish);

			var retrieved = await _repository.GetById(dish.Id);
			retrieved!.Name = "Updated Name";
			retrieved.Price = 25.00m;

			// Act
			await _repository.Commit();

			// Assert
			var updated = await _dbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dish.Id);
			updated.Should().NotBeNull();
			updated!.Name.Should().Be("Updated Name");
			updated.Price.Should().Be(25.00m);
		}

		[Theory]
		[InlineData(DishCategory.Appetizer)]
		[InlineData(DishCategory.Soup)]
		[InlineData(DishCategory.MainCourse)]
		[InlineData(DishCategory.Dessert)]
		[InlineData(DishCategory.Beverage)]
		[InlineData(DishCategory.Salad)]
		[InlineData(DishCategory.Pizza)]
		[InlineData(DishCategory.Pasta)]
		public async Task Create_DifferentCategories_SavesCorrectly(DishCategory category)
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dish = new Dish
			{
				Name = $"{category} Dish",
				Description = "Test",
				Price = 10.00m,
				Category = category,
				RestaurantId = restaurant.Id
			};

			// Act
			await _repository.Create(dish);

			// Assert
			var saved = await _dbContext.Dishes.FirstOrDefaultAsync(d => d.Id == dish.Id);
			saved.Should().NotBeNull();
			saved!.Category.Should().Be(category);
		}

		[Fact]
		public async Task Create_MultipleDishesForSameRestaurant_AllSaved()
		{
			// Arrange
			var restaurant = await CreateRestaurant();
			var dishes = new[]
			{
				new Dish { Name = "Dish 1", Description = "Test", Price = 10, Category = DishCategory.Appetizer, RestaurantId = restaurant.Id },
				new Dish { Name = "Dish 2", Description = "Test", Price = 20, Category = DishCategory.MainCourse, RestaurantId = restaurant.Id },
				new Dish { Name = "Dish 3", Description = "Test", Price = 30, Category = DishCategory.Dessert, RestaurantId = restaurant.Id }
			};

			// Act
			foreach (var dish in dishes)
			{
				await _repository.Create(dish);
			}

			// Assert
			var allDishes = await _dbContext.Dishes.Where(d => d.RestaurantId == restaurant.Id).ToListAsync();
			allDishes.Should().HaveCount(3);
		}

		private async Task<Domain.Entities.Restaurant> CreateRestaurant()
		{
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test Restaurant",
				Description = "Test Description",
				Category = "Italian",
				HasDelivery = true,
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				OwnerId = "test-owner",
				CreatedAt = DateTime.UtcNow,
				Address = new Address
				{
					City = "Warsaw",
					Street = "Test Street",
					PostalCode = "00-001"
				}
			};
			restaurant.EncodeName();

			_dbContext.Restaurants.Add(restaurant);
			await _dbContext.SaveChangesAsync();

			return restaurant;
		}

		public void Dispose()
		{
			_dbContext.Database.EnsureDeleted();
			_dbContext.Dispose();
		}
	}
}
