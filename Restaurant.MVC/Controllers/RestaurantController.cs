using Application.RestaurantDto.Commands.CreateRestaurant;
using Application.RestaurantDto.Commands.EditRestaurant;
using Application.RestaurantDto.Queries.GetAllRestaurants;
using Application.RestaurantDto.Queries.GetRestaurantByEncodedName;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

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

        public async Task<IActionResult> Index()
        {
            var restaurants = await _mediator.Send(new GetAllRestaurantsQuery());
            return View(restaurants);
        }

        public IActionResult Create()
        {
            return View();
        }

        [Route("Restaurant/{encodedName}/Detalis")]
        public async Task<IActionResult> Details(string encodedName)
        {
            var dto = await _mediator.Send(new GetRestaurantByEncodedNameQuery(encodedName));

            return View(dto);
        }

        [Route("Restaurant/{encodedName}/Edit")]
        public async Task<IActionResult> Edit(string encodedName)
        {
            var dto = await _mediator.Send(new GetRestaurantByEncodedNameQuery(encodedName));

            EditRestaurantCommand model = _mapper.Map<EditRestaurantCommand>(dto);

            return View(model);
        }

        [HttpPost]
        [Route("Restaurant/{encodedName}/Edit")]
        public async Task<IActionResult> Edit(string encodedName, EditRestaurantCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }
            await _mediator.Send(command);
            return RedirectToAction(nameof(Create));
        }
    }
}
