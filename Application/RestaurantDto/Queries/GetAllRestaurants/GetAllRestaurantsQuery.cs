using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RestaurantDto.Queries.GetAllRestaurants
{
    public class GetAllRestaurantsQuery : IRequest<IEnumerable<RestaurantDto>>
    {
    }
}
