using Application.AdminDto.Queries.GetAdminStatistics;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Restaurant.Tests.Application.Queries
{
	public class GetAdminStatisticsQueryHandlerTests : IDisposable
	{
		private readonly RestaurantDbContext _dbContext;
		private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
		private readonly GetAdminStatisticsQueryHandler _handler;

		public GetAdminStatisticsQueryHandlerTests()
		{
			var options = new DbContextOptionsBuilder<RestaurantDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_dbContext = new RestaurantDbContext(options);

			var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
			_userManagerMock = new Mock<UserManager<ApplicationUser>>(
				userStoreMock.Object, null, null, null, null, null, null, null, null);

			_handler = new GetAdminStatisticsQueryHandler(_dbContext, _userManagerMock.Object);
		}

		[Fact]
		public async Task Handle_EmptyDatabase_ReturnsZeroStatistics()
		{
			// Arrange
			var users = new List<ApplicationUser>().AsQueryable();
			var mockSet = CreateMockDbSet(users);

			_userManagerMock.Setup(u => u.Users).Returns(mockSet.Object);

			var query = new GetAdminStatisticsQuery();

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.TotalRestaurants.Should().Be(0);
			result.TotalUsers.Should().Be(0);
			result.TotalDishes.Should().Be(0);
			result.RestaurantsWithDelivery.Should().Be(0);
			result.RestaurantsCreatedThisMonth.Should().Be(0);
		}

		[Fact]
		public async Task Handle_WithRestaurants_ReturnsCorrectCount()
		{
			// Arrange
			await SeedRestaurants(5);

			var users = new List<ApplicationUser>().AsQueryable();
			var mockSet = CreateMockDbSet(users);
			_userManagerMock.Setup(u => u.Users).Returns(mockSet.Object);

			var query = new GetAdminStatisticsQuery();

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.TotalRestaurants.Should().Be(5);
		}

		[Fact]
		public async Task Handle_WithDelivery_CountsCorrectly()
		{
			// Arrange
			await SeedRestaurants(3, hasDelivery: true);
			await SeedRestaurants(2, hasDelivery: false);

			var users = new List<ApplicationUser>().AsQueryable();
			var mockSet = CreateMockDbSet(users);
			_userManagerMock.Setup(u => u.Users).Returns(mockSet.Object);

			var query = new GetAdminStatisticsQuery();

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.TotalRestaurants.Should().Be(5);
			result.RestaurantsWithDelivery.Should().Be(3);
		}

		[Fact]
		public async Task Handle_RestaurantsCreatedThisMonth_CountsCorrectly()
		{
			// Arrange
			var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			var lastMonth = thisMonth.AddMonths(-1);

			await SeedRestaurantsWithDate(3, thisMonth.AddDays(5));
			await SeedRestaurantsWithDate(2, lastMonth);

			var users = new List<ApplicationUser>().AsQueryable();
			var mockSet = CreateMockDbSet(users);
			_userManagerMock.Setup(u => u.Users).Returns(mockSet.Object);

			var query = new GetAdminStatisticsQuery();

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.RestaurantsCreatedThisMonth.Should().Be(3);
		}

		[Fact]
		public async Task Handle_WithDishes_CountsCorrectly()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test Restaurant",
				Category = "Italian",
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				Address = new Address { City = "Warsaw", Street = "Test", PostalCode = "00-001" }
			};
			restaurant.EncodeName();
			_dbContext.Restaurants.Add(restaurant);
			await _dbContext.SaveChangesAsync();

			_dbContext.Dishes.Add(new Dish
			{
				Name = "Dish 1",
				Description = "Test",
				Price = 10,
				RestaurantId = restaurant.Id
			});
			_dbContext.Dishes.Add(new Dish
			{
				Name = "Dish 2",
				Description = "Test",
				Price = 20,
				RestaurantId = restaurant.Id
			});
			await _dbContext.SaveChangesAsync();

			var users = new List<ApplicationUser>().AsQueryable();
			var mockSet = CreateMockDbSet(users);
			_userManagerMock.Setup(u => u.Users).Returns(mockSet.Object);

			var query = new GetAdminStatisticsQuery();

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.TotalDishes.Should().Be(2);
		}

		private async Task SeedRestaurants(int count, bool hasDelivery = false)
		{
			for (int i = 0; i < count; i++)
			{
				var restaurant = new Domain.Entities.Restaurant
				{
					Name = $"Restaurant {i}",
					Category = "Italian",
					HasDelivery = hasDelivery,
					ContactEmail = $"test{i}@test.com",
					ContactNumber = "123456789",
					CreatedAt = DateTime.UtcNow,
					Address = new Address { City = "Warsaw", Street = "Test", PostalCode = "00-001" }
				};
				restaurant.EncodeName();
				_dbContext.Restaurants.Add(restaurant);
			}
			await _dbContext.SaveChangesAsync();
		}

		private async Task SeedRestaurantsWithDate(int count, DateTime createdAt)
		{
			for (int i = 0; i < count; i++)
			{
				var restaurant = new Domain.Entities.Restaurant
				{
					Name = $"Restaurant {Guid.NewGuid()}",
					Category = "Italian",
					ContactEmail = $"test{Guid.NewGuid()}@test.com",
					ContactNumber = "123456789",
					CreatedAt = createdAt,
					Address = new Address { City = "Warsaw", Street = "Test", PostalCode = "00-001" }
				};
				restaurant.EncodeName();
				_dbContext.Restaurants.Add(restaurant);
			}
			await _dbContext.SaveChangesAsync();
		}

		private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
		{
			var mockSet = new Mock<DbSet<T>>();
			mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
			mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
			mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
			mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
			return mockSet;
		}

		public void Dispose()
		{
			_dbContext.Database.EnsureDeleted();
			_dbContext.Dispose();
		}
	}
}
