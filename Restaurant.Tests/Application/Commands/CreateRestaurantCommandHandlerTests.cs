using Application.RestaurantDto.Commands.CreateRestaurant;
using AutoMapper;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Restaurant.Tests.Application.Commands
{
	public class CreateRestaurantCommandHandlerTests
	{
		private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock;
		private readonly Mock<IMapper> _mapperMock;
		private readonly CreateRestaurantCommandHandler _handler;

		public CreateRestaurantCommandHandlerTests()
		{
			_restaurantRepositoryMock = new Mock<IRestaurantRepository>();
			_mapperMock = new Mock<IMapper>();
			_handler = new CreateRestaurantCommandHandler(_restaurantRepositoryMock.Object, _mapperMock.Object);
		}

		[Fact]
		public async Task Handle_ValidCommand_CreatesRestaurantWithCorrectData()
		{
			// Arrange
			var command = new CreateRestaurantCommand
			{
				Name = "Test Restaurant",
				Description = "Test Description",
				Category = "Italian",
				HasDelivery = true,
				City = "Warsaw",
				Street = "Test Street",
				PostalCode = "00-001",
				ContactEmail = "test@restaurant.com",
				ContactNumber = "123456789",
				OwnerId = "user123"
			};

			var restaurant = new Domain.Entities.Restaurant
			{
				Name = command.Name,
				Description = command.Description,
				Category = command.Category,
				HasDelivery = command.HasDelivery,
				ContactEmail = command.ContactEmail,
				ContactNumber = command.ContactNumber
			};

			_mapperMock.Setup(m => m.Map<Domain.Entities.Restaurant>(command))
				.Returns(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			_restaurantRepositoryMock.Verify(r => r.Create(It.Is<Domain.Entities.Restaurant>(
				x => x.Name == command.Name &&
					 x.OwnerId == command.OwnerId &&
					 x.CreatedAt != default(DateTime)
			)), Times.Once);
		}

		[Fact]
		public async Task Handle_ValidCommand_EncodesRestaurantName()
		{
			// Arrange
			var command = new CreateRestaurantCommand
			{
				Name = "Test Restaurant With Spaces",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test Street",
				PostalCode = "00-001",
				ContactEmail = "test@restaurant.com",
				ContactNumber = "123456789"
			};

			var restaurant = new Domain.Entities.Restaurant
			{
				Name = command.Name
			};

			_mapperMock.Setup(m => m.Map<Domain.Entities.Restaurant>(command))
				.Returns(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			_restaurantRepositoryMock.Verify(r => r.Create(It.Is<Domain.Entities.Restaurant>(
				x => !string.IsNullOrEmpty(x.EncodedName)
			)), Times.Once);
		}

		[Fact]
		public async Task Handle_ValidCommand_SetsOwnerIdCorrectly()
		{
			// Arrange
			var expectedOwnerId = "owner-123";
			var command = new CreateRestaurantCommand
			{
				Name = "Test Restaurant",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test Street",
				PostalCode = "00-001",
				ContactEmail = "test@restaurant.com",
				ContactNumber = "123456789",
				OwnerId = expectedOwnerId
			};

			var restaurant = new Domain.Entities.Restaurant { Name = command.Name };
			_mapperMock.Setup(m => m.Map<Domain.Entities.Restaurant>(command)).Returns(restaurant);

			// Act
			await _handler.Handle(command, CancellationToken.None);

			// Assert
			_restaurantRepositoryMock.Verify(r => r.Create(It.Is<Domain.Entities.Restaurant>(
				x => x.OwnerId == expectedOwnerId
			)), Times.Once);
		}

		[Fact]
		public async Task Handle_ValidCommand_SetsCreatedAtToUtcNow()
		{
			// Arrange
			var command = new CreateRestaurantCommand
			{
				Name = "Test Restaurant",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test Street",
				PostalCode = "00-001",
				ContactEmail = "test@restaurant.com",
				ContactNumber = "123456789"
			};

			var restaurant = new Domain.Entities.Restaurant { Name = command.Name };
			_mapperMock.Setup(m => m.Map<Domain.Entities.Restaurant>(command)).Returns(restaurant);

			var beforeExecution = DateTime.UtcNow;

			// Act
			await _handler.Handle(command, CancellationToken.None);

			var afterExecution = DateTime.UtcNow;

			// Assert
			_restaurantRepositoryMock.Verify(r => r.Create(It.Is<Domain.Entities.Restaurant>(
				x => x.CreatedAt >= beforeExecution && x.CreatedAt <= afterExecution
			)), Times.Once);
		}
	}
}
