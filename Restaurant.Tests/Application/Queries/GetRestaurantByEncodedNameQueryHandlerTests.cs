using Application.RestaurantDto.Queries.GetRestaurantByEncodedName;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Restaurant.Tests.Application.Queries
{
	public class GetRestaurantByEncodedNameQueryHandlerTests
	{
		private readonly Mock<IRestaurantRepository> _restaurantRepositoryMock;
		private readonly Mock<IMapper> _mapperMock;
		private readonly GetRestaurantByEncodedNameQueryHandler _handler;

		public GetRestaurantByEncodedNameQueryHandlerTests()
		{
			_restaurantRepositoryMock = new Mock<IRestaurantRepository>();
			_mapperMock = new Mock<IMapper>();
			_handler = new GetRestaurantByEncodedNameQueryHandler(_restaurantRepositoryMock.Object, _mapperMock.Object);
		}

		[Fact]
		public async Task Handle_ValidEncodedName_ReturnsRestaurantDto()
		{
			// Arrange
			var encodedName = "test-restaurant";
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test Restaurant",
				EncodedName = encodedName,
				Category = "Italian",
				Address = new Address
				{
					City = "Warsaw",
					Street = "Test Street",
					PostalCode = "00-001"
				}
			};

			var restaurantDto = new Application.RestaurantDto.RestaurantDto
			{
				Name = restaurant.Name,
				EncodedName = restaurant.EncodedName,
				Category = restaurant.Category,
				City = "Warsaw",
				Street = "Test Street",
				PostalCode = "00-001"
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(encodedName))
				.ReturnsAsync(restaurant);

			_mapperMock.Setup(m => m.Map<Application.RestaurantDto.RestaurantDto>(restaurant))
				.Returns(restaurantDto);

			var query = new GetRestaurantByEncodedNameQuery(encodedName);

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Should().Be(restaurantDto);
			result.Name.Should().Be(restaurant.Name);
			result.EncodedName.Should().Be(encodedName);
		}

		[Fact]
		public async Task Handle_RestaurantNotFound_ReturnsNull()
		{
			// Arrange
			var encodedName = "non-existent";

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(encodedName))
				.ReturnsAsync((Domain.Entities.Restaurant?)null);

			var query = new GetRestaurantByEncodedNameQuery(encodedName);

			// Act
			var result = await _handler.Handle(query, CancellationToken.None);

			// Assert
			result.Should().BeNull();
			_mapperMock.Verify(m => m.Map<Application.RestaurantDto.RestaurantDto>(It.IsAny<Domain.Entities.Restaurant>()), Times.Never);
		}

		[Fact]
		public async Task Handle_ValidQuery_CallsRepositoryOnce()
		{
			// Arrange
			var encodedName = "test-restaurant";
			var restaurant = new Domain.Entities.Restaurant
			{
				Id = 1,
				Name = "Test",
				EncodedName = encodedName,
				Address = new Address()
			};

			_restaurantRepositoryMock.Setup(r => r.GetByEncodedName(encodedName))
				.ReturnsAsync(restaurant);

			_mapperMock.Setup(m => m.Map<Application.RestaurantDto.RestaurantDto>(restaurant))
				.Returns(new Application.RestaurantDto.RestaurantDto());

			var query = new GetRestaurantByEncodedNameQuery(encodedName);

			// Act
			await _handler.Handle(query, CancellationToken.None);

			// Assert
			_restaurantRepositoryMock.Verify(r => r.GetByEncodedName(encodedName), Times.Once);
		}
	}
}
