using Domain.Enums;

namespace Application.RestaurantDto
{
    public class DishDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DishCategory Category { get; set; }
    }
}
