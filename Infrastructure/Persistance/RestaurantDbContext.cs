using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance
{
	public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
            
        }

        public DbSet<Restaurant> Restaurants { get; set;}
        public DbSet<Address> Addresses { get; set;}
        public DbSet<Dish> Dishes { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(25);

            modelBuilder.Entity<Dish>()
                .Property(d => d.Name)
                .IsRequired();

            modelBuilder.Entity<Dish>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,4)");
        }
    }
}
