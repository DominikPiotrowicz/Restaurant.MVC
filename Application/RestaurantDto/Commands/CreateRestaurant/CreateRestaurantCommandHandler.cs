using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.RestaurantDto.Commands.CreateRestaurant
{
    public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand>
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMapper _mapper;

        public CreateRestaurantCommandHandler(IRestaurantRepository restaurantRepository, IMapper mapper)
        {
            _restaurantRepository = restaurantRepository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = _mapper.Map<Domain.Entities.Restaurant>(request);
            restaurant.EncodeName();

            await _restaurantRepository.Create(restaurant);

            return Unit.Value;
        }
    }
}
