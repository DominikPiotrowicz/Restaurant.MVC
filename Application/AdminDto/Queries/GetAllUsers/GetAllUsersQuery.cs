using MediatR;

namespace Application.AdminDto.Queries.GetAllUsers
{
	public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
	{
	}
}
