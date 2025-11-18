using Domain.Entities;
using Infrastructure.Persistance;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminDto.Queries.GetAllUsers
{
	public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RestaurantDbContext _dbContext;

		public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager, RestaurantDbContext dbContext)
		{
			_userManager = userManager;
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
		{
			var users = await _userManager.Users.ToListAsync(cancellationToken);
			var userDtos = new List<UserDto>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				var restaurantsCount = await _dbContext.Restaurants.CountAsync(r => r.OwnerId == user.Id, cancellationToken);

				userDtos.Add(new UserDto
				{
					Id = user.Id,
					Email = user.Email!,
					FirstName = user.FirstName,
					LastName = user.LastName,
					EmailConfirmed = user.EmailConfirmed,
					Roles = roles,
					RestaurantsCount = restaurantsCount
				});
			}

			return userDtos;
		}
	}
}
