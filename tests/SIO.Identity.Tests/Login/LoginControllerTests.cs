using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SIO.Identity.Login;
using SIO.Identity.Login.Requests;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests.Login
{
    public class LoginControllerTests : ControllerTest<LoginController>
    {
        [Fact]
        public async Task Login_GET_Should_Redirect_When_Return_Url_Is_Not_Null()
        {
            var controller = BuildController(out var serviceProvider);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            var mockReturnUrl = "http://localhost/something";

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(uh => uh.IsLocalUrl(It.Is<string>(s => s == mockReturnUrl))).Returns(true);

            controller.Url = mockUrlHelper.Object;

            var result = await controller.Login(mockReturnUrl);
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be(mockReturnUrl);
        }

        [Fact]
        public async Task Login_GET_Should_Return_View_When_UnAuthenticated()
        {
            var controller = BuildController(out var serviceProvider);
            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.SetupGet(mu => mu.Identity.IsAuthenticated).Returns(false);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = mockUser.Object
                }
            };

            var result = await controller.Login("");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_Email_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "", Password = "asdf" };
            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_Email_Is_Invalid()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "invalid", Password = "asdf" };
            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_Email_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = null, Password = "asdf" };
            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_Password_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "test@sound-it-out.com", Password = "" };
            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_Password_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "test@sound-it-out.com", Password = null };
            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_User_Doesnt_Exist()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "test@sound-it-out.com", Password = "asdf" };
            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_User_Is_Locked_Out()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "test@sound-it-out.com", Password = "asdf" };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = request.Email,
            };

            await userManager.CreateAsync(user);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddDays(1));

            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_User_Is_Not_Verified()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "test@sound-it-out.com", Password = "asdf" };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = request.Email,
            };

            await userManager.CreateAsync(user);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_POST_Should_Error_When_Incorrect_Password_Is_Supplied()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new LoginRequest { Email = "test@sound-it-out.com", Password = "asdf" };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = request.Email,
            };

            await userManager.CreateAsync(user);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
            await userManager.AddPasswordAsync(user, "fdsa");            

            controller.ValidateRequest(request);

            var result = await controller.Login(request, "login");
            user = await userManager.FindByEmailAsync(user.Email);
            user.AccessFailedCount.Should().BeGreaterThan(0);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }
    }
}
