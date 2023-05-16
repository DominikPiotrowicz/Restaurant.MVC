using Application.RestaurantDto;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.MVC.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RestaurantDto restaurant)
        {
            await _restaurantService.Create(restaurant);
            return RedirectToAction(nameof(Create)); //TODO: Refactor
        }

    }
}
