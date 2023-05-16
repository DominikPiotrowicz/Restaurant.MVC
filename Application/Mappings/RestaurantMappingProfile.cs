using Application.RestaurantDto;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class RestaurantMappingProfile : Profile
    {
        public RestaurantMappingProfile()
        {
            CreateMap<RestaurantDto.RestaurantDto, Restaurant>()
                .ForMember(e => e.Address, opt => opt.MapFrom(src => new Address()
                {
                    City = src.City,
                    Street = src.Street,
                    PostalCode = src.PostalCode
                }));
                

            CreateMap<Dish, DishDto>();
        }
    }
}
