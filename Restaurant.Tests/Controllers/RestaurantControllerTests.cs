using Application.RestaurantDto;
using Application.RestaurantDto.Commands.CreateRestaurant;
using Application.RestaurantDto.Commands.EditRestaurant;
using Application.RestaurantDto.Queries.GetRestaurantByEncodedName;
using Application.RestaurantDto.Queries.SearchRestaurants;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Restaurant.MVC.Controllers;
using System.Security.Claims;
using Xunit;

namespace Restaurant.Tests.Controllers
{
	public class RestaurantControllerTests
	{
		private readonly Mock<IMediator> _mediatorMock;
		private readonly Mock<IMapper> _mapperMock;
		private readonly RestaurantController _controller;

		public RestaurantControllerTests()
		{
			_mediatorMock = new Mock<IMediator>();
			_mapperMock = new Mock<IMapper>();
			_controller = new RestaurantController(_mediatorMock.Object, _mapperMock.Object);
		}

		[Fact]
		public async Task Index_NoParameters_SendsSearchQuery()
		{
			// Arrange
			var restaurants = new List<RestaurantDto>
			{
				new RestaurantDto { Name = "Restaurant 1" },
				new RestaurantDto { Name = "Restaurant 2" }
			};

			_mediatorMock.Setup(m => m.Send(It.IsAny<SearchRestaurantsQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(restaurants);

			// Act
			var result = await _controller.Index(null, null, null);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<RestaurantDto>>().Subject;
			model.Should().HaveCount(2);
		}

		[Fact]
		public async Task Index_WithSearchPhrase_PassesToQuery()
		{
			// Arrange
			var searchPhrase = "Pizza";
			SearchRestaurantsQuery? capturedQuery = null;

			_mediatorMock.Setup(m => m.Send(It.IsAny<SearchRestaurantsQuery>(), It.IsAny<CancellationToken>()))
				.Callback<IRequest<IEnumerable<RestaurantDto>>, CancellationToken>((query, _) =>
				{
					capturedQuery = query as SearchRestaurantsQuery;
				})
				.ReturnsAsync(new List<RestaurantDto>());

			// Act
			await _controller.Index(searchPhrase, null, null);

			// Assert
			capturedQuery.Should().NotBeNull();
			capturedQuery!.SearchPhrase.Should().Be(searchPhrase);
		}

		[Fact]
		public void Create_Get_ReturnsView()
		{
			// Arrange
			SetupUserClaims();

			// Act
			var result = _controller.Create();

			// Assert
			result.Should().BeOfType<ViewResult>();
		}

		[Fact]
		public async Task Create_Post_ValidModel_RedirectsToIndex()
		{
			// Arrange
			var userId = "user-123";
			SetupUserClaims(userId);

			var command = new CreateRestaurantCommand
			{
				Name = "New Restaurant",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test",
				PostalCode = "00-001",
				ContactEmail = "test@test.com",
				ContactNumber = "123456789"
			};

			// Act
			var result = await _controller.Create(command);

			// Assert
			var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
			redirectResult.ActionName.Should().Be("Index");

			_mediatorMock.Verify(m => m.Send(It.Is<CreateRestaurantCommand>(
				c => c.OwnerId == userId
			), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task Create_Post_InvalidModel_ReturnsView()
		{
			// Arrange
			SetupUserClaims();
			_controller.ModelState.AddModelError("Name", "Required");

			var command = new CreateRestaurantCommand();

			// Act
			var result = await _controller.Create(command);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			viewResult.Model.Should().Be(command);
		}

		[Fact]
		public async Task Details_ValidEncodedName_ReturnsViewWithRestaurant()
		{
			// Arrange
			var encodedName = "test-restaurant";
			var restaurantDto = new RestaurantDto
			{
				Name = "Test Restaurant",
				EncodedName = encodedName
			};

			_mediatorMock.Setup(m => m.Send(It.IsAny<GetRestaurantByEncodedNameQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(restaurantDto);

			// Act
			var result = await _controller.Details(encodedName);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			var model = viewResult.Model.Should().BeOfType<RestaurantDto>().Subject;
			model.EncodedName.Should().Be(encodedName);
		}

		[Fact]
		public async Task Edit_Get_ValidEncodedName_ReturnsViewWithCommand()
		{
			// Arrange
			SetupUserClaims();
			var encodedName = "test-restaurant";
			var restaurantDto = new RestaurantDto { Name = "Test", EncodedName = encodedName };
			var editCommand = new EditRestaurantCommand { Name = "Test" };

			_mediatorMock.Setup(m => m.Send(It.IsAny<GetRestaurantByEncodedNameQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(restaurantDto);

			_mapperMock.Setup(m => m.Map<EditRestaurantCommand>(restaurantDto))
				.Returns(editCommand);

			// Act
			var result = await _controller.Edit(encodedName);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			viewResult.Model.Should().Be(editCommand);
		}

		[Fact]
		public async Task Edit_Post_ValidModel_RedirectsToIndex()
		{
			// Arrange
			var userId = "user-123";
			SetupUserClaims(userId);

			var command = new EditRestaurantCommand
			{
				EncodedName = "test-restaurant",
				Name = "Updated Restaurant",
				Category = "Italian",
				City = "Warsaw",
				Street = "Test",
				PostalCode = "00-001",
				ContactEmail = "test@test.com",
				ContactNumber = "123456789"
			};

			// Act
			var result = await _controller.Edit("test-restaurant", command);

			// Assert
			var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
			redirectResult.ActionName.Should().Be("Index");

			_mediatorMock.Verify(m => m.Send(It.Is<EditRestaurantCommand>(
				c => c.CurrentUserId == userId
			), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task Edit_Post_InvalidModel_ReturnsView()
		{
			// Arrange
			SetupUserClaims();
			_controller.ModelState.AddModelError("Name", "Required");

			var command = new EditRestaurantCommand();

			// Act
			var result = await _controller.Edit("test", command);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			viewResult.Model.Should().Be(command);
		}

		private void SetupUserClaims(string userId = "test-user")
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Name, "test@test.com")
			};

			var identity = new ClaimsIdentity(claims, "TestAuthType");
			var claimsPrincipal = new ClaimsPrincipal(identity);

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = claimsPrincipal }
			};
		}
	}
}
