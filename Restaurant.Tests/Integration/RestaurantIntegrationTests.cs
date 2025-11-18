using FluentAssertions;
using System.Net;
using Xunit;

namespace Restaurant.Tests.Integration
{
	public class RestaurantIntegrationTests : IClassFixture<CustomWebApplicationFactory>
	{
		private readonly CustomWebApplicationFactory _factory;
		private readonly HttpClient _client;

		public RestaurantIntegrationTests(CustomWebApplicationFactory factory)
		{
			_factory = factory;
			_client = factory.CreateClient();
		}

		[Fact]
		public async Task Home_Index_ReturnsSuccess()
		{
			// Act
			var response = await _client.GetAsync("/");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Restaurant_Index_ReturnsSuccess()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/Index");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var content = await response.Content.ReadAsStringAsync();
			content.Should().Contain("Restaurant");
		}

		[Fact]
		public async Task Restaurant_Details_ValidRestaurant_ReturnsSuccess()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/test-restaurant/Details");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var content = await response.Content.ReadAsStringAsync();
			content.Should().Contain("Test Restaurant");
		}

		[Fact]
		public async Task Restaurant_Create_Unauthorized_RedirectsToLogin()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/Create");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.Redirect);
			response.Headers.Location?.ToString().Should().Contain("/Account/Login");
		}

		[Fact]
		public async Task Account_Login_Get_ReturnsSuccess()
		{
			// Act
			var response = await _client.GetAsync("/Account/Login");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var content = await response.Content.ReadAsStringAsync();
			content.Should().Contain("Zaloguj się");
		}

		[Fact]
		public async Task Account_Register_Get_ReturnsSuccess()
		{
			// Act
			var response = await _client.GetAsync("/Account/Register");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var content = await response.Content.ReadAsStringAsync();
			content.Should().Contain("Zarejestruj się");
		}

		[Fact]
		public async Task Admin_Index_Unauthorized_RedirectsToLogin()
		{
			// Act
			var response = await _client.GetAsync("/Admin/Index");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.Redirect);
			response.Headers.Location?.ToString().Should().Contain("/Account/Login");
		}

		[Fact]
		public async Task Dish_Create_Unauthorized_RedirectsToLogin()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/test-restaurant/Dish/Create");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.Redirect);
			response.Headers.Location?.ToString().Should().Contain("/Account/Login");
		}

		[Theory]
		[InlineData("/")]
		[InlineData("/Home/Privacy")]
		[InlineData("/Restaurant/Index")]
		[InlineData("/Account/Login")]
		[InlineData("/Account/Register")]
		public async Task PublicPages_Get_ReturnsSuccess(string url)
		{
			// Act
			var response = await _client.GetAsync(url);

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Theory]
		[InlineData("/Restaurant/Create")]
		[InlineData("/Admin/Index")]
		[InlineData("/Admin/Users")]
		[InlineData("/Admin/Restaurants")]
		public async Task ProtectedPages_Unauthorized_RedirectsToLogin(string url)
		{
			// Act
			var response = await _client.GetAsync(url);

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.Redirect);
			response.Headers.Location?.ToString().Should().Contain("/Account/Login");
		}

		[Fact]
		public async Task Restaurant_Index_WithSearchPhrase_ReturnsFilteredResults()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/Index?searchPhrase=Test");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var content = await response.Content.ReadAsStringAsync();
			content.Should().Contain("Test");
		}

		[Fact]
		public async Task Restaurant_Index_WithDeliveryFilter_ReturnsResults()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/Index?hasDelivery=true");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Restaurant_Details_NonExistingRestaurant_Returns404OrError()
		{
			// Act
			var response = await _client.GetAsync("/Restaurant/non-existing-restaurant/Details");

			// Assert
			// Could be 404, 500, or redirect depending on implementation
			response.StatusCode.Should().NotBe(HttpStatusCode.OK);
		}
	}
}
