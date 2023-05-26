using Application.RestaurantDto;
using Application.RestaurantDto.Commands.CreateRestaurant;
using Application.RestaurantDto.Queries.GetAllRestaurants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.MVC.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IMediator _mediator;

        public RestaurantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var restaurants = await _mediator.Send(new GetAllRestaurantsQuery());
            return View(restaurants);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantCommand command)
        {   /*
            if (!ModelState.IsValid)    //TODO: Refactor Validation Model State
            {                   
                return View(restaurant);
            }*/
            await _mediator.Send(command);
            return RedirectToAction(nameof(Create));
        }
    }
}
