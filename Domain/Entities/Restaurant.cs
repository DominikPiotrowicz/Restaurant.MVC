namespace Domain.Entities
{
	public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Category { get; set; }
        public bool HasDelivery { get; set; }
        public string ContactEmail { get; set; }
        public string ContactNumber { get; set; }

        public Address Address { get; set; } = default!;

        public string EncodedName { get; private set; } = default!;

        public int AddressId { get; set; }
        //public virtual Address Address { get; set; }
        public virtual List<Dish> Dishes { get; set; }

        public void EncodeName() => EncodedName = Name.ToLower().Replace(" ", "-").ToString();
    }
}
