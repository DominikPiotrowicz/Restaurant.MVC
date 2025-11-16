using Domain.Enums;

namespace Application.Extensions
{
	public static class EnumExtensions
	{
		public static string ToPolishString(this DishCategory category)
		{
			return category switch
			{
				DishCategory.Appetizer => "Przystawka",
				DishCategory.Soup => "Zupa",
				DishCategory.MainCourse => "Danie główne",
				DishCategory.Dessert => "Deser",
				DishCategory.Beverage => "Napój",
				DishCategory.Salad => "Sałatka",
				DishCategory.Pizza => "Pizza",
				DishCategory.Pasta => "Makaron",
				_ => category.ToString()
			};
		}
	}
}
