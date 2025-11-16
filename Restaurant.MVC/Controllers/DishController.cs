using Application.DishDto.Commands.CreateDish;
using Application.DishDto.Commands.DeleteDish;
using Application.DishDto.Commands.EditDish;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Restaurant.MVC.Controllers
{
	[Authorize]
	public class DishController : Controller
	{
		private readonly IMediator _mediator;
		private readonly IMapper _mapper;

		public DishController(IMediator mediator, IMapper mapper)
		{
			_mediator = mediator;
			_mapper = mapper;
		}

		[Route("Restaurant/{restaurantEncodedName}/Dish/Create")]
		public IActionResult Create(string restaurantEncodedName)
		{
			var model = new CreateDishCommand
			{
				RestaurantEncodedName = restaurantEncodedName
			};
			return View(model);
		}

		[HttpPost]
		[Route("Restaurant/{restaurantEncodedName}/Dish/Create")]
		public async Task<IActionResult> Create(string restaurantEncodedName, CreateDishCommand command)
		{
			if (!ModelState.IsValid)
			{
				return View(command);
			}

			command.RestaurantEncodedName = restaurantEncodedName;
			command.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			await _mediator.Send(command);

			return RedirectToAction("Details", "Restaurant", new { encodedName = restaurantEncodedName });
		}

		[Route("Restaurant/{restaurantEncodedName}/Dish/{dishId}/Edit")]
		public async Task<IActionResult> Edit(string restaurantEncodedName, int dishId)
		{
			var dish = await _mediator.Send(new Application.DishDto.Queries.GetDishById.GetDishByIdQuery(dishId));

			if (dish == null)
			{
				return NotFound();
			}

			var model = _mapper.Map<EditDishCommand>(dish);
			model.RestaurantEncodedName = restaurantEncodedName;

			return View(model);
		}

		[HttpPost]
		[Route("Restaurant/{restaurantEncodedName}/Dish/{dishId}/Edit")]
		public async Task<IActionResult> Edit(string restaurantEncodedName, int dishId, EditDishCommand command)
		{
			if (!ModelState.IsValid)
			{
				return View(command);
			}

			command.Id = dishId;
			command.RestaurantEncodedName = restaurantEncodedName;
			command.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			await _mediator.Send(command);

			return RedirectToAction("Details", "Restaurant", new { encodedName = restaurantEncodedName });
		}

		[HttpPost]
		[Route("Restaurant/{restaurantEncodedName}/Dish/{dishId}/Delete")]
		public async Task<IActionResult> Delete(string restaurantEncodedName, int dishId)
		{
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			await _mediator.Send(new DeleteDishCommand(dishId, restaurantEncodedName, currentUserId));

			return RedirectToAction("Details", "Restaurant", new { encodedName = restaurantEncodedName });
		}
	}
}
