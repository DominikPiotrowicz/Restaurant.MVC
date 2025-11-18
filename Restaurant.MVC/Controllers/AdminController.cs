using Application.AdminDto.Queries.GetAdminStatistics;
using Application.AdminDto.Queries.GetAllUsers;
using Application.RestaurantDto.Queries.GetAllRestaurants;
using Domain.Entities;
using Domain.Interfaces;
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
		private readonly IAuditLogService _auditLogService;

		public AdminController(
			IMediator mediator,
			UserManager<ApplicationUser> userManager,
			IAuditLogService auditLogService)
		{
			_mediator = mediator;
			_userManager = userManager;
			_auditLogService = auditLogService;
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
				await _auditLogService.LogAsync(
					"Admin.RemoveAdminRole",
					"ApplicationUser",
					user.Id,
					new { Role = "Administrator", Email = user.Email },
					null,
					true);

				TempData["Success"] = $"Usunięto rolę administratora dla użytkownika {user.Email}";
			}
			else
			{
				await _userManager.AddToRoleAsync(user, "Administrator");
				await _auditLogService.LogAsync(
					"Admin.AddAdminRole",
					"ApplicationUser",
					user.Id,
					null,
					new { Role = "Administrator", Email = user.Email },
					true);

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
				await _auditLogService.LogAsync(
					"Admin.DeleteUser",
					"ApplicationUser",
					userId,
					null,
					null,
					false,
					"User not found");
				return NotFound();
			}

			// Nie pozwalaj na usunięcie samego siebie
			if (user.Id == _userManager.GetUserId(User))
			{
				await _auditLogService.LogAsync(
					"Admin.DeleteUser",
					"ApplicationUser",
					user.Id,
					new { Email = user.Email },
					null,
					false,
					"Cannot delete own account");

				TempData["Error"] = "Nie możesz usunąć własnego konta administratora";
				return RedirectToAction(nameof(Users));
			}

			var userEmail = user.Email;
			var result = await _userManager.DeleteAsync(user);

			if (result.Succeeded)
			{
				await _auditLogService.LogAsync(
					"Admin.DeleteUser",
					"ApplicationUser",
					userId,
					new { Email = userEmail },
					null,
					true);

				TempData["Success"] = $"Użytkownik {userEmail} został usunięty";
			}
			else
			{
				await _auditLogService.LogAsync(
					"Admin.DeleteUser",
					"ApplicationUser",
					userId,
					new { Email = userEmail },
					null,
					false,
					string.Join("; ", result.Errors.Select(e => e.Description)));

				TempData["Error"] = "Błąd podczas usuwania użytkownika";
			}

			return RedirectToAction(nameof(Users));
		}
	}
}
