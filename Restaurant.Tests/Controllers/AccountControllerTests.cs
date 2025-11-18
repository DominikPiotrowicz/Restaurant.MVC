using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Restaurant.MVC.Controllers;
using Restaurant.MVC.Models.Account;
using Xunit;

namespace Restaurant.Tests.Controllers
{
	public class AccountControllerTests
	{
		private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
		private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
		private readonly AccountController _controller;

		public AccountControllerTests()
		{
			var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
			_userManagerMock = new Mock<UserManager<ApplicationUser>>(
				userStoreMock.Object, null, null, null, null, null, null, null, null);

			var contextAccessorMock = new Mock<IHttpContextAccessor>();
			var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

			_signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
				_userManagerMock.Object,
				contextAccessorMock.Object,
				userPrincipalFactoryMock.Object,
				null, null, null, null);

			_controller = new AccountController(_userManagerMock.Object, _signInManagerMock.Object);
		}

		[Fact]
		public void Register_Get_ReturnsView()
		{
			// Act
			var result = _controller.Register();

			// Assert
			result.Should().BeOfType<ViewResult>();
		}

		[Fact]
		public async Task Register_Post_ValidModel_CreatesUserAndRedirects()
		{
			// Arrange
			var model = new RegisterViewModel
			{
				Email = "newuser@test.com",
				Password = "Test123!",
				ConfirmPassword = "Test123!",
				FirstName = "John",
				LastName = "Doe"
			};

			_userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);

			_userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
				.ReturnsAsync(IdentityResult.Success);

			_signInManagerMock.Setup(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
				.Returns(Task.CompletedTask);

			// Act
			var result = await _controller.Register(model);

			// Assert
			var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
			redirectResult.ActionName.Should().Be("Index");
			redirectResult.ControllerName.Should().Be("Home");

			_userManagerMock.Verify(u => u.CreateAsync(
				It.Is<ApplicationUser>(user =>
					user.Email == model.Email &&
					user.FirstName == model.FirstName &&
					user.LastName == model.LastName
				),
				model.Password
			), Times.Once);

			_userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
			_signInManagerMock.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false, null), Times.Once);
		}

		[Fact]
		public async Task Register_Post_UserCreationFails_ReturnsViewWithErrors()
		{
			// Arrange
			var model = new RegisterViewModel
			{
				Email = "test@test.com",
				Password = "Test123!",
				ConfirmPassword = "Test123!"
			};

			var errors = new[]
			{
				new IdentityError { Description = "Email already exists" }
			};

			_userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Failed(errors));

			// Act
			var result = await _controller.Register(model);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			viewResult.Model.Should().Be(model);
			_controller.ModelState.Should().ContainKey(string.Empty);
		}

		[Fact]
		public async Task Register_Post_InvalidModel_ReturnsView()
		{
			// Arrange
			var model = new RegisterViewModel();
			_controller.ModelState.AddModelError("Email", "Required");

			// Act
			var result = await _controller.Register(model);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			viewResult.Model.Should().Be(model);

			_userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public void Login_Get_ReturnsViewWithReturnUrl()
		{
			// Arrange
			var returnUrl = "/Restaurant/Index";

			// Act
			var result = _controller.Login(returnUrl);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			var model = viewResult.Model.Should().BeOfType<LoginViewModel>().Subject;
			model.ReturnUrl.Should().Be(returnUrl);
		}

		[Fact]
		public async Task Login_Post_ValidCredentials_SignsInAndRedirects()
		{
			// Arrange
			var model = new LoginViewModel
			{
				Email = "user@test.com",
				Password = "Test123!",
				RememberMe = false
			};

			_signInManagerMock.Setup(s => s.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					false))
				.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

			// Act
			var result = await _controller.Login(model);

			// Assert
			var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
			redirectResult.ActionName.Should().Be("Index");
			redirectResult.ControllerName.Should().Be("Home");
		}

		[Fact]
		public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
		{
			// Arrange
			var model = new LoginViewModel
			{
				Email = "user@test.com",
				Password = "WrongPassword",
				RememberMe = false
			};

			_signInManagerMock.Setup(s => s.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					false))
				.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

			// Act
			var result = await _controller.Login(model);

			// Assert
			var viewResult = result.Should().BeOfType<ViewResult>().Subject;
			viewResult.Model.Should().Be(model);
			_controller.ModelState.Should().ContainKey(string.Empty);
		}

		[Fact]
		public async Task Login_Post_WithValidReturnUrl_RedirectsToReturnUrl()
		{
			// Arrange
			var returnUrl = "/Restaurant/Create";
			var model = new LoginViewModel
			{
				Email = "user@test.com",
				Password = "Test123!",
				ReturnUrl = returnUrl
			};

			_signInManagerMock.Setup(s => s.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					false))
				.ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			_controller.Url = new MockUrlHelper(returnUrl, true);

			// Act
			var result = await _controller.Login(model);

			// Assert
			var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
			redirectResult.Url.Should().Be(returnUrl);
		}

		[Fact]
		public async Task Logout_SignsOutAndRedirects()
		{
			// Arrange
			_signInManagerMock.Setup(s => s.SignOutAsync())
				.Returns(Task.CompletedTask);

			// Act
			var result = await _controller.Logout();

			// Assert
			var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
			redirectResult.ActionName.Should().Be("Index");
			redirectResult.ControllerName.Should().Be("Home");

			_signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
		}

		[Fact]
		public void AccessDenied_ReturnsView()
		{
			// Act
			var result = _controller.AccessDenied();

			// Assert
			result.Should().BeOfType<ViewResult>();
		}

		// Helper class to mock IUrlHelper
		private class MockUrlHelper : IUrlHelper
		{
			private readonly string _returnUrl;
			private readonly bool _isLocalUrl;

			public MockUrlHelper(string returnUrl, bool isLocalUrl)
			{
				_returnUrl = returnUrl;
				_isLocalUrl = isLocalUrl;
			}

			public ActionContext ActionContext => throw new NotImplementedException();

			public string Action(UrlActionContext actionContext) => throw new NotImplementedException();
			public string Content(string contentPath) => throw new NotImplementedException();
			public bool IsLocalUrl(string url) => _isLocalUrl;
			public string Link(string routeName, object values) => throw new NotImplementedException();
			public string RouteUrl(UrlRouteContext routeContext) => throw new NotImplementedException();
		}
	}
}
