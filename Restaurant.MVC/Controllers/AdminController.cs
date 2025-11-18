using Application.AdminDto.Queries.GetAdminStatistics;
using Application.AdminDto.Queries.GetAllUsers;
using Application.RestaurantDto.Queries.GetAllRestaurants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.MVC.Controllers
{
	[Authorize(Roles = "Administrator")]
	public class AdminController : Controller
	{
		private readonly IMediator _mediator;
		private readonly UserManager<ApplicationUser> _userManager;

		public AdminController(IMediator mediator, UserManager<ApplicationUser> userManager)
		{
			_mediator = mediator;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var statistics = await _mediator.Send(new GetAdminStatisticsQuery());
			return View(statistics);
		}

		public async Task<IActionResult> Users()
		{
			var users = await _mediator.Send(new GetAllUsersQuery());
			return View(users);
		}

		public async Task<IActionResult> Restaurants()
		{
			var restaurants = await _mediator.Send(new GetAllRestaurantsQuery());
			return View(restaurants);
		}

		[HttpPost]
		public async Task<IActionResult> ToggleAdminRole(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound();
			}

			var isAdmin = await _userManager.IsInRoleAsync(user, "Administrator");

			if (isAdmin)
			{
				await _userManager.RemoveFromRoleAsync(user, "Administrator");
				TempData["Success"] = $"Usunięto rolę administratora dla użytkownika {user.Email}";
			}
			else
			{
				await _userManager.AddToRoleAsync(user, "Administrator");
				TempData["Success"] = $"Dodano rolę administratora dla użytkownika {user.Email}";
			}

			return RedirectToAction(nameof(Users));
		}

		[HttpPost]
		public async Task<IActionResult> DeleteUser(string userId)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound();
			}

			// Nie pozwalaj na usunięcie samego siebie
			if (user.Id == _userManager.GetUserId(User))
			{
				TempData["Error"] = "Nie możesz usunąć własnego konta administratora";
				return RedirectToAction(nameof(Users));
			}

			var result = await _userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				TempData["Success"] = $"Użytkownik {user.Email} został usunięty";
			}
			else
			{
				TempData["Error"] = "Błąd podczas usuwania użytkownika";
			}

			return RedirectToAction(nameof(Users));
		}
	}
}
