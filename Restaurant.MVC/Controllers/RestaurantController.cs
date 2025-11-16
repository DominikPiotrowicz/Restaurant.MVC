using Application.RestaurantDto.Commands.CreateRestaurant;
using Application.RestaurantDto.Commands.EditRestaurant;
using Application.RestaurantDto.Queries.GetAllRestaurants;
using Application.RestaurantDto.Queries.GetRestaurantByEncodedName;
using Application.RestaurantDto.Queries.SearchRestaurants;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Restaurant.MVC.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public RestaurantController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? searchPhrase, string? category, bool? hasDelivery)
        {
            var query = new SearchRestaurantsQuery
            {
                SearchPhrase = searchPhrase,
                Category = category,
                HasDelivery = hasDelivery
            };

            var restaurants = await _mediator.Send(query);

            // Przekazanie parametrów do ViewBag dla formularza
            ViewBag.SearchPhrase = searchPhrase;
            ViewBag.Category = category;
            ViewBag.HasDelivery = hasDelivery;

            return View(restaurants);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Restaurant/{encodedName}/Details")]
        public async Task<IActionResult> Details(string encodedName)
        {
            var dto = await _mediator.Send(new GetRestaurantByEncodedNameQuery(encodedName));

            return View(dto);
        }

        [Authorize]
        [Route("Restaurant/{encodedName}/Edit")]
        public async Task<IActionResult> Edit(string encodedName)
        {
            var dto = await _mediator.Send(new GetRestaurantByEncodedNameQuery(encodedName));

            EditRestaurantCommand model = _mapper.Map<EditRestaurantCommand>(dto);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [Route("Restaurant/{encodedName}/Edit")]
        public async Task<IActionResult> Edit(string encodedName, EditRestaurantCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            command.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            command.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
    }
}
