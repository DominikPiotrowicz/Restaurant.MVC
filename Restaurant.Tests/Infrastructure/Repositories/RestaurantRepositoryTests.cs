using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistance;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Restaurant.Tests.Infrastructure.Repositories
{
	public class RestaurantRepositoryTests : IDisposable
	{
		private readonly RestaurantDbContext _dbContext;
		private readonly RestaurantRepository _repository;

		public RestaurantRepositoryTests()
		{
			var options = new DbContextOptionsBuilder<RestaurantDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_dbContext = new RestaurantDbContext(options);
			_repository = new RestaurantRepository(_dbContext);
		}

		[Fact]
		public async Task Create_ValidRestaurant_AddsToDatabase()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test Restaurant",
				Description = "Test Description",
				Category = "Italian",
				HasDelivery = true,
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				Address = new Address
				{
					City = "Warsaw",
					Street = "Test Street",
					PostalCode = "00-001"
				}
			};
			restaurant.EncodeName();

			// Act
			await _repository.Create(restaurant);

			// Assert
			var savedRestaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurant.Id);
			savedRestaurant.Should().NotBeNull();
			savedRestaurant!.Name.Should().Be(restaurant.Name);
			savedRestaurant.Category.Should().Be(restaurant.Category);
		}

		[Fact]
		public async Task GetAll_MultipleRestaurants_ReturnsAll()
		{
			// Arrange
			var restaurants = new[]
			{
				CreateRestaurant("Restaurant 1"),
				CreateRestaurant("Restaurant 2"),
				CreateRestaurant("Restaurant 3")
			};

			foreach (var restaurant in restaurants)
			{
				await _repository.Create(restaurant);
			}

			// Act
			var result = await _repository.GetAll();

			// Assert
			result.Should().HaveCount(3);
			result.Select(r => r.Name).Should().Contain(new[] { "Restaurant 1", "Restaurant 2", "Restaurant 3" });
		}

		[Fact]
		public async Task GetByEncodedName_ExistingRestaurant_ReturnsRestaurant()
		{
			// Arrange
			var restaurant = CreateRestaurant("Test Restaurant");
			await _repository.Create(restaurant);

			// Act
			var result = await _repository.GetByEncodedName(restaurant.EncodedName);

			// Assert
			result.Should().NotBeNull();
			result!.Name.Should().Be(restaurant.Name);
			result.EncodedName.Should().Be(restaurant.EncodedName);
		}

		[Fact]
		public async Task GetByEncodedName_NonExistingRestaurant_ReturnsNull()
		{
			// Arrange
			var encodedName = "non-existent-restaurant";

			// Act
			var result = await _repository.GetByEncodedName(encodedName);

			// Assert
			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByEncodedName_IncludesAddressAndDishes()
		{
			// Arrange
			var restaurant = CreateRestaurant("Test Restaurant");
			await _repository.Create(restaurant);

			_dbContext.Dishes.Add(new Dish
			{
				Name = "Test Dish",
				Description = "Test",
				Price = 10,
				RestaurantId = restaurant.Id
			});
			await _dbContext.SaveChangesAsync();

			// Act
			var result = await _repository.GetByEncodedName(restaurant.EncodedName);

			// Assert
			result.Should().NotBeNull();
			result!.Address.Should().NotBeNull();
			result.Dishes.Should().HaveCount(1);
			result.Dishes.First().Name.Should().Be("Test Dish");
		}

		[Fact]
		public async Task Commit_UpdatesChanges_SavesSuccessfully()
		{
			// Arrange
			var restaurant = CreateRestaurant("Original Name");
			await _repository.Create(restaurant);

			var retrieved = await _repository.GetByEncodedName(restaurant.EncodedName);
			retrieved!.Name = "Updated Name";

			// Act
			await _repository.Commit();

			// Assert
			var updated = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurant.Id);
			updated.Should().NotBeNull();
			updated!.Name.Should().Be("Updated Name");
		}

		[Fact]
		public async Task Create_RestaurantWithOwner_SavesOwnerId()
		{
			// Arrange
			var ownerId = "owner-123";
			var restaurant = CreateRestaurant("Test Restaurant");
			restaurant.OwnerId = ownerId;

			// Act
			await _repository.Create(restaurant);

			// Assert
			var saved = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurant.Id);
			saved.Should().NotBeNull();
			saved!.OwnerId.Should().Be(ownerId);
		}

		[Fact]
		public async Task GetAll_IncludesAddressAndDishes()
		{
			// Arrange
			var restaurant = CreateRestaurant("Test Restaurant");
			await _repository.Create(restaurant);

			_dbContext.Dishes.Add(new Dish
			{
				Name = "Dish 1",
				Description = "Test",
				Price = 15,
				RestaurantId = restaurant.Id
			});
			await _dbContext.SaveChangesAsync();

			// Act
			var result = await _repository.GetAll();

			// Assert
			result.Should().HaveCount(1);
			var first = result.First();
			first.Address.Should().NotBeNull();
			first.Dishes.Should().HaveCount(1);
		}

		private Domain.Entities.Restaurant CreateRestaurant(string name)
		{
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = name,
				Description = "Test Description",
				Category = "Italian",
				HasDelivery = true,
				ContactEmail = $"{name.Replace(" ", "")}@test.com",
				ContactNumber = "123456789",
				CreatedAt = DateTime.UtcNow,
				Address = new Address
				{
					City = "Warsaw",
					Street = "Test Street",
					PostalCode = "00-001"
				}
			};
			restaurant.EncodeName();
			return restaurant;
		}

		public void Dispose()
		{
			_dbContext.Database.EnsureDeleted();
			_dbContext.Dispose();
		}
	}
}
