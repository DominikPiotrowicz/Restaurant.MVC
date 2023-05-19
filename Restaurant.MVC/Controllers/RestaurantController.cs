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

        public async Task<IActionResult> Index()
        {
            var restaurants = await _restaurantService.GetAll();
            return View(restaurants);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RestaurantDto restaurant)
        {   /*
            if (!ModelState.IsValid)    //TODO: Refactor Validation Model State
            {                   
                return View(restaurant);
            }*/
            await _restaurantService.Create(restaurant);
            return RedirectToAction(nameof(Create)); //TODO: Refactor
        }
    }
}
