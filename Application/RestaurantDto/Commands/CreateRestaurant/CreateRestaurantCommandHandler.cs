using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.RestaurantDto.Commands.CreateRestaurant
{
    public class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand>
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;

        public CreateRestaurantCommandHandler(
            IRestaurantRepository restaurantRepository,
            IMapper mapper,
            IAuditLogService auditLogService)
        {
            _restaurantRepository = restaurantRepository;
            _mapper = mapper;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var restaurant = _mapper.Map<Domain.Entities.Restaurant>(request);
            restaurant.EncodeName();
            restaurant.CreatedAt = DateTime.UtcNow;
            restaurant.OwnerId = request.OwnerId;

            await _restaurantRepository.Create(restaurant);

            await _auditLogService.LogAsync(
                "Restaurant.Create",
                "Restaurant",
                restaurant.Id.ToString(),
                null,
                new { Name = restaurant.Name, Category = restaurant.Category, OwnerId = restaurant.OwnerId },
                true);

            return Unit.Value;
        }
    }
}
