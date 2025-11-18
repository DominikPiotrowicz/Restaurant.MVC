using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.MVC.Models.Account;

namespace Restaurant.MVC.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ISecurityMonitoringService _securityMonitoring;
		private readonly IAuditLogService _auditLogService;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ISecurityMonitoringService securityMonitoring,
			IAuditLogService auditLogService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_securityMonitoring = securityMonitoring;
			_auditLogService = auditLogService;
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				FirstName = model.FirstName,
				LastName = model.LastName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, "User");
				await _auditLogService.LogAsync(
					"User.Register",
					"ApplicationUser",
					user.Id,
					null,
					new { Email = user.Email, FirstName = user.FirstName, LastName = user.LastName },
					true);

				await _signInManager.SignInAsync(user, isPersistent: false);
				return RedirectToAction("Index", "Restaurant");
			}

			await _auditLogService.LogAsync(
				"User.Register",
				"ApplicationUser",
				null,
				null,
				new { Email = model.Email },
				false,
				string.Join("; ", result.Errors.Select(e => e.Description)));

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			var model = new LoginViewModel { ReturnUrl = returnUrl };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Sprawdź czy konto nie jest zablokowane
			if (await _securityMonitoring.IsAccountLockedAsync(model.Email))
			{
				ModelState.AddModelError(string.Empty,
					"Konto zostało tymczasowo zablokowane z powodu zbyt wielu nieudanych prób logowania. Spróbuj ponownie za 15 minut.");
				return View(model);
			}

			var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				await _securityMonitoring.LogLoginAttemptAsync(model.Email, true);

				if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
				{
					return Redirect(model.ReturnUrl);
				}
				return RedirectToAction("Index", "Restaurant");
			}

			await _securityMonitoring.LogLoginAttemptAsync(model.Email, false, "Nieprawidłowy email lub hasło");

			ModelState.AddModelError(string.Empty, "Nieprawidłowy email lub hasło");
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			var userEmail = User.Identity?.Name;
			await _signInManager.SignOutAsync();

			if (!string.IsNullOrEmpty(userEmail))
			{
				await _auditLogService.LogAsync(
					"User.Logout",
					"ApplicationUser",
					userEmail,
					null,
					null,
					true);
			}

			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}
	}
}
