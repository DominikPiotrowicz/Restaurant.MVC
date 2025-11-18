using Application.DishDto.Commands.CreateDish;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Restaurant.Tests.Application.Commands
{
	public class CreateDishCommandHandlerTests
	{
		private readonly Mock<IDishRepository> _dishRepositoryMock;
		private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock;
		private readonly CreateDishCommandHandler _handler;

		public CreateDishCommandHandlerTests()
		{
			_dishRepositoryMock = new Mock<IDishRepository>();
			_restaurantRepositoryMock = new Mock<IRestaurantRepository>();
			_handler = new CreateDishCommandHandler(_dishRepositoryMock.Object, _restaurantRepositoryMock.Object);
		}

		[Fact]
		public async Task Handle_ValidCommand_CreatesDishWithCorrectData()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test Restaurant",
				OwnerId = "owner123"
			};

			var command = new CreateDishCommand
			{
				Name = "Pizza Margherita",
				Description = "Classic Italian pizza",
				Price = 25.99m,
				Category = DishCategory.Pizza,
				RestaurantEncodedName = "test-restaurant",
				CurrentUserId = "owner123"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.RestaurantEncodedName))
				.ReturnsAsync(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			_dishRepositoryMock.Verify(d => d.Create(It.Is<Dish>(
				dish => dish.Name == command.Name &&
						dish.Description == command.Description &&
						dish.Price == command.Price &&
						dish.Category == command.Category &&
						dish.RestaurantId == restaurant.Id
			)), Times.Once);
		}

		[Fact]
		public async Task Handle_RestaurantNotFound_ThrowsInvalidOperationException()
		{
			// Arrange
			var command = new CreateDishCommand
			{
				Name = "Pizza",
				Description = "Test",
				Price = 10.00m,
				Category = DishCategory.Pizza,
				RestaurantEncodedName = "non-existent",
				CurrentUserId = "user123"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.RestaurantEncodedName))
				.ReturnsAsync((Domain.Entities.Restaurant?)null);

			// Act & Assert
			var act = async () => await _handler.Handle(command, CancellationToken.None);

			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage($"Restaurant with encoded name '{command.RestaurantEncodedName}' not found.");
		}

		[Fact]
		public async Task Handle_UserIsNotOwner_ThrowsUnauthorizedAccessException()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test Restaurant",
				OwnerId = "owner123"
			};

			var command = new CreateDishCommand
			{
				Name = "Pizza",
				Description = "Test",
				Price = 10.00m,
				Category = DishCategory.Pizza,
				RestaurantEncodedName = "test-restaurant",
				CurrentUserId = "different-user"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.RestaurantEncodedName))
				.ReturnsAsync(restaurant);

			// Act & Assert
			var act = async () => await _handler.Handle(command, CancellationToken.None);

			await act.Should().ThrowAsync<UnauthorizedAccessException>()
				.WithMessage("Only the restaurant owner can add dishes.");
		}

		[Fact]
		public async Task Handle_UserIsOwner_AllowsCreation()
		{
			// Arrange
			var ownerId = "owner123";
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test Restaurant",
				OwnerId = ownerId
			};

			var command = new CreateDishCommand
			{
				Name = "Pasta Carbonara",
				Description = "Creamy pasta",
				Price = 30.00m,
				Category = DishCategory.Pasta,
				RestaurantEncodedName = "test-restaurant",
				CurrentUserId = ownerId
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.RestaurantEncodedName))
				.ReturnsAsync(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			_dishRepositoryMock.Verify(d => d.Create(It.IsAny<Dish>()), Times.Once);
		}

		[Theory]
		[InlineData(DishCategory.Appetizer)]
		[InlineData(DishCategory.MainCourse)]
		[InlineData(DishCategory.Dessert)]
		[InlineData(DishCategory.Beverage)]
		public async Task Handle_DifferentCategories_CreatesCorrectly(DishCategory category)
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				OwnerId = "owner123"
			};

			var command = new CreateDishCommand
			{
				Name = "Test Dish",
				Description = "Test",
				Price = 20.00m,
				Category = category,
				RestaurantEncodedName = "test-restaurant",
				CurrentUserId = "owner123"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.RestaurantEncodedName))
				.ReturnsAsync(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			_dishRepositoryMock.Verify(d => d.Create(It.Is<Dish>(
				dish => dish.Category == category
			)), Times.Once);
		}
	}
}
