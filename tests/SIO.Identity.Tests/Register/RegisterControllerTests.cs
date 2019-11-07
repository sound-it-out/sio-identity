using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SIO.Identity.Register;
using SIO.Identity.Register.Requests;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests.Register
{
    public class RegisterControllerTests : ControllerTest<RegisterController>
    {
        [Fact]
        public void Register_GET_Should_Redirect_To_Default_App_Url_When_User_Is_Authenticated()
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

            var result = controller.Register();
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be("DefaultAppUrl");
        }

        [Fact]
        public void Register_GET_Should_Return_View_When_User_Is_UnAuthenticated()
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

            var result = controller.Register();
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Registered_GET_Should_Redirect_To_Default_App_Url_When_User_Is_Authenticated()
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

            var result = controller.Registered();
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be("DefaultAppUrl");
        }

        [Fact]
        public void Registered_GET_Should_Return_View_When_User_Is_UnAuthenticated()
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

            var result = controller.Registered();
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Register_POST_Errors_When_Email_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = null,
                FirstName = "FirstName",
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_Email_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "",
                FirstName = "FirstName",
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_Email_Is_Invalid()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "FirstName",
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_FirstName_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = null,
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_FirstName_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "",
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_LastName_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "FirstName",
                LastName = null
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_lastName_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "FirstName",
                LastName = ""
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Returns_View_When_Request_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var result = await controller.Register(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Register_POST_Should_Error_When_User_Is_Verified()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

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

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Should_Error_When_User_Is_Not_Verified()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

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

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Should_Redirect_To_Registered_If_Registration_Successful()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(RegisterController.Registered));
            controller.ModelState.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Register_POST_Should_Return_Error_When_User_Creation_Fails()
        {
            var controller = BuildController(out var serviceProvider);
            var userManger = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            ((MockUserManager)userManger).InterceptCreateUser = true;

            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }
    }
}
