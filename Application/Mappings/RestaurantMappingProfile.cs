﻿using Application.RestaurantDto;
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

            CreateMap<Restaurant, RestaurantDto.RestaurantDto>()
                .ForMember(dto => dto.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dto => dto.Street, opt => opt.MapFrom(src => src.Address.Street))
                .ForMember(dto => dto.PostalCode, opt => opt.MapFrom(src => src.Address.PostalCode));


			CreateMap<Dish, DishDto>();
        }
    }
}
