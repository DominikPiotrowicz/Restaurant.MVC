using Application.RestaurantDto.Commands.EditRestaurant;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Restaurant.Tests.Application.Commands
{
	public class EditRestaurantCommandHandlerTests
	{
		private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock;
		private readonly EditRestaurantCommandHandler _handler;

		public EditRestaurantCommandHandlerTests()
		{
			_restaurantRepositoryMock = new Mock<IRestaurantRepository>();
			_handler = new EditRestaurantCommandHandler(_restaurantRepositoryMock.Object);
		}

		[Fact]
		public async Task Handle_ValidCommand_UpdatesRestaurantData()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Old Name",
				Description = "Old Description",
				Category = "Old Category",
				HasDelivery = false,
				ContactEmail = "old@email.com",
				ContactNumber = "111111111",
				OwnerId = "owner123",
				Address = new Address
				{
					City = "Old City",
					Street = "Old Street",
					PostalCode = "00-000"
				}
			};

			var command = new EditRestaurantCommand
			{
				EncodedName = "old-name",
				Name = "New Name",
				Description = "New Description",
				Category = "New Category",
				HasDelivery = true,
				ContactEmail = "new@email.com",
				ContactNumber = "999999999",
				City = "New City",
				Street = "New Street",
				PostalCode = "11-111",
				CurrentUserId = "owner123"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.EncodedName))
				.ReturnsAsync(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			restaurant.Name.Should().Be(command.Name);
			restaurant.Description.Should().Be(command.Description);
			restaurant.Category.Should().Be(command.Category);
			restaurant.HasDelivery.Should().Be(command.HasDelivery);
			restaurant.ContactEmail.Should().Be(command.ContactEmail);
			restaurant.ContactNumber.Should().Be(command.ContactNumber);
			restaurant.Address.City.Should().Be(command.City);
			restaurant.Address.Street.Should().Be(command.Street);
			restaurant.Address.PostalCode.Should().Be(command.PostalCode);

			_restaurantRepositoryMock.Verify(r => r.Commit(), Times.Once);
		}

		[Fact]
		public async Task Handle_RestaurantNotFound_ThrowsInvalidOperationException()
		{
			// Arrange
			var command = new EditRestaurantCommand
			{
				EncodedName = "non-existent",
				Name = "Test",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test",
				PostalCode = "00-001",
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				CurrentUserId = "user123"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.EncodedName))
				.ReturnsAsync((Domain.Entities.Restaurant?)null);

			// Act & Assert
			var act = async () => await _handler.Handle(command, CancellationToken.None);

			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage($"Restaurant with encoded name '{command.EncodedName}' not found.");
		}

		[Fact]
		public async Task Handle_UserIsNotOwner_ThrowsUnauthorizedAccessException()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test Restaurant",
				OwnerId = "owner123",
				Address = new Address()
			};

			var command = new EditRestaurantCommand
			{
				EncodedName = "test-restaurant",
				Name = "Updated Name",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test",
				PostalCode = "00-001",
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				CurrentUserId = "different-user"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.EncodedName))
				.ReturnsAsync(restaurant);

			// Act & Assert
			var act = async () => await _handler.Handle(command, CancellationToken.None);

			await act.Should().ThrowAsync<UnauthorizedAccessException>()
				.WithMessage("Only the restaurant owner can edit this restaurant.");
		}

		[Fact]
		public async Task Handle_UserIsOwner_AllowsEdit()
		{
			// Arrange
			var ownerId = "owner123";
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test Restaurant",
				OwnerId = ownerId,
				Address = new Address
				{
					City = "Warsaw",
					Street = "Old Street",
					PostalCode = "00-000"
				}
			};

			var command = new EditRestaurantCommand
			{
				EncodedName = "test-restaurant",
				Name = "Updated Name",
				Description = "Updated Description",
				Category = "Italian",
				HasDelivery = true,
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				City = "Krakow",
				Street = "New Street",
				PostalCode = "11-111",
				CurrentUserId = ownerId
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(command.EncodedName))
				.ReturnsAsync(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			restaurant.Name.Should().Be(command.Name);
			_restaurantRepositoryMock.Verify(r => r.Commit(), Times.Once);
		}
	}
}
