using Application.Extensions;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Restaurant.Tests.Application.Extensions
{
	public class EnumExtensionsTests
	{
		[Theory]
		[InlineData(DishCategory.Appetizer, "Przystawka")]
		[InlineData(DishCategory.Soup, "Zupa")]
		[InlineData(DishCategory.MainCourse, "Danie główne")]
		[InlineData(DishCategory.Dessert, "Deser")]
		[InlineData(DishCategory.Beverage, "Napój")]
		[InlineData(DishCategory.Salad, "Sałatka")]
		[InlineData(DishCategory.Pizza, "Pizza")]
		[InlineData(DishCategory.Pasta, "Makaron")]
		public void ToPolishString_ValidCategory_ReturnsCorrectTranslation(DishCategory category, string expected)
		{
			// Act
			var result = category.ToPolishString();

			// Assert
			result.Should().Be(expected);
		}

		[Fact]
		public void ToPolishString_AllCategories_ReturnNonEmptyStrings()
		{
			// Arrange
			var allCategories = Enum.GetValues<DishCategory>();

			// Act & Assert
			foreach (var category in allCategories)
			{
				var result = category.ToPolishString();
				result.Should().NotBeNullOrEmpty();
			}
		}

		[Fact]
		public void ToPolishString_Appetizer_ReturnsPolishTranslation()
		{
			// Arrange
			var category = DishCategory.Appetizer;

			// Act
			var result = category.ToPolishString();

			// Assert
			result.Should().Be("Przystawka");
			result.Should().NotContain("Appetizer");
		}

		[Fact]
		public void ToPolishString_MainCourse_ReturnsPolishTranslation()
		{
			// Arrange
			var category = DishCategory.MainCourse;

			// Act
			var result = category.ToPolishString();

			// Assert
			result.Should().Be("Danie główne");
			result.Should().Contain("główne");
		}
	}
}
