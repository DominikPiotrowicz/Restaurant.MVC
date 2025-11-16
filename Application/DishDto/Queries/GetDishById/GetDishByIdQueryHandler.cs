using Application.RestaurantDto;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.DishDto.Queries.GetDishById
{
	public class GetDishByIdQueryHandler : IRequestHandler<GetDishByIdQuery, DishDto?>
	{
		private readonly IDishRepository _dishRepository;
		private readonly IMapper _mapper;

		public GetDishByIdQueryHandler(IDishRepository dishRepository, IMapper mapper)
		{
			_dishRepository = dishRepository;
			_mapper = mapper;
		}

		public async Task<DishDto?> Handle(GetDishByIdQuery request, CancellationToken cancellationToken)
		{
			var dish = await _dishRepository.GetById(request.Id);

			if (dish == null)
			{
				return null;
			}

			var dto = _mapper.Map<DishDto>(dish);

			return dto;
		}
	}
}
