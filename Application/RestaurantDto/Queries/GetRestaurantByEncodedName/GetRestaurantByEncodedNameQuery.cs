using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RestaurantDto.Queries.GetRestaurantByEncodedName
{
	public class GetRestaurantByEncodedNameQuery : IRequest<RestaurantDto>
	{
		public string EncodedName { get; set; }

        public GetRestaurantByEncodedNameQuery(string encodedName)
        {
                EncodedName = encodedName;
        }
    }
}
