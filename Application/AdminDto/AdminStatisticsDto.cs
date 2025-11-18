namespace Application.AdminDto
{
	public class AdminStatisticsDto
	{
		public int TotalRestaurants { get; set; }
		public int TotalUsers { get; set; }
		public int TotalDishes { get; set; }
		public int RestaurantsWithDelivery { get; set; }
		public int RestaurantsCreatedThisMonth { get; set; }
	}
}
