using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Restaurant.Tests.Domain
{
	public class RestaurantTests
	{
		[Fact]
		public void EncodeName_ValidName_EncodesCorrectly()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test Restaurant Name"
			};

			// Act
			restaurant.EncodeName();

			// Assert
			restaurant.EncodedName.Should().Be("test-restaurant-name");
		}

		[Fact]
		public void EncodeName_NameWithSpecialCharacters_EncodesCorrectly()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Pizza & Pasta Restaurant!"
			};

			// Act
			restaurant.EncodeName();

			// Assert
			restaurant.EncodedName.Should().NotBeNullOrEmpty();
			restaurant.EncodedName.Should().NotContain(" ");
			restaurant.EncodedName.Should().NotContain("&");
			restaurant.EncodedName.Should().NotContain("!");
		}

		[Fact]
		public void EncodeName_NameWithPolishCharacters_EncodesCorrectly()
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Restauracja Śródziemnomorska"
			};

			// Act
			restaurant.EncodeName();

			// Assert
			restaurant.EncodedName.Should().NotBeNullOrEmpty();
			restaurant.EncodedName.Should().NotContain("ś");
			restaurant.EncodedName.Should().NotContain("ó");
		}

		[Theory]
		[InlineData("ABC Restaurant", "abc-restaurant")]
		[InlineData("123 Street Food", "123-street-food")]
		[InlineData("  Spaces  ", "spaces")]
		public void EncodeName_VariousNames_EncodesAsExpected(string name, string expected)
		{
			// Arrange
			var restaurant = new Domain.Entities.Restaurant { Name = name };

			// Act
			restaurant.EncodeName();

			// Assert
			restaurant.EncodedName.Should().Be(expected);
		}

		[Fact]
		public void Restaurant_Properties_SetCorrectly()
		{
			// Arrange & Act
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test Restaurant",
				Description = "A wonderful place",
				Category = "Italian",
				HasDelivery = true,
				ContactEmail = "test@test.com",
				ContactNumber = "123456789",
				OwnerId = "owner123",
				CreatedAt = DateTime.UtcNow
			};

			// Assert
			restaurant.Name.Should().Be("Test Restaurant");
			restaurant.Description.Should().Be("A wonderful place");
			restaurant.Category.Should().Be("Italian");
			restaurant.HasDelivery.Should().BeTrue();
			restaurant.ContactEmail.Should().Be("test@test.com");
			restaurant.ContactNumber.Should().Be("123456789");
			restaurant.OwnerId.Should().Be("owner123");
			restaurant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
		}

		[Fact]
		public void Restaurant_WithAddress_RelationshipWorks()
		{
			// Arrange & Act
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test",
				Address = new Address
				{
					City = "Warsaw",
					Street = "Main Street",
					PostalCode = "00-001"
				}
			};

			// Assert
			restaurant.Address.Should().NotBeNull();
			restaurant.Address.City.Should().Be("Warsaw");
			restaurant.Address.Street.Should().Be("Main Street");
			restaurant.Address.PostalCode.Should().Be("00-001");
		}

		[Fact]
		public void Restaurant_WithDishes_RelationshipWorks()
		{
			// Arrange & Act
			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test",
				Dishes = new List<Dish>
				{
					new Dish { Name = "Dish 1", Price = 10.00m },
					new Dish { Name = "Dish 2", Price = 20.00m }
				}
			};

			// Assert
			restaurant.Dishes.Should().HaveCount(2);
			restaurant.Dishes.Should().Contain(d => d.Name == "Dish 1");
			restaurant.Dishes.Should().Contain(d => d.Name == "Dish 2");
		}

		[Fact]
		public void Restaurant_WithOwner_RelationshipWorks()
		{
			// Arrange & Act
			var ownerId = "user-123";
			var owner = new ApplicationUser
			{
				Id = ownerId,
				Email = "owner@test.com",
				FirstName = "John",
				LastName = "Doe"
			};

			var restaurant = new Domain.Entities.Restaurant
			{
				Name = "Test",
				OwnerId = ownerId,
				Owner = owner
			};

			// Assert
			restaurant.OwnerId.Should().Be(ownerId);
			restaurant.Owner.Should().NotBeNull();
			restaurant.Owner.Email.Should().Be("owner@test.com");
		}

		[Fact]
		public void Restaurant_DefaultValues_SetCorrectly()
		{
			// Arrange & Act
			var restaurant = new Domain.Entities.Restaurant();

			// Assert
			restaurant.Dishes.Should().NotBeNull();
			restaurant.Dishes.Should().BeEmpty();
		}
	}
}
