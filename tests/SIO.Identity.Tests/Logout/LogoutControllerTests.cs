using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SIO.Identity.Logout;
using SIO.Identity.Logout.Requests;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests.Logout
{
    public class LogoutControllerTests : ControllerTests<LogoutController>
    {
        [Fact]
        public void Logout_GET_Should_Return_View_When_User_Is_Authenticated_And_LogoutId_Is_Empty()
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

            var result = controller.Logout("");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Logout_GET_Should_Return_View_When_User_Is_Authenticated_And_LogoutId_Is_Valid()
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

            var result = controller.Logout("id");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Logout_GET_Should_Redirect_To_LoggedOut_When_UnAuthenticated()
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

            var result = controller.Logout("");
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(LogoutController.LoggedOut));
        }

        [Fact]
        public async Task Logout_POST_Should_Redirect_To_LoggedOut_When_Authenticated_And_Idp_Is_Not_Null_And_Logout_Id_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@sound-it-out.com",
                FirstName = "firstname",
                LastName = "lastname",
                UserName = "test@sound-it-out.com",
            };

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(JwtClaimTypes.IdentityProvider, "asdf"),
                        new Claim(JwtClaimTypes.Subject, user.Id),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            await userManager.CreateAsync(user);

            var result = await controller.Logout(new LogoutRequest());
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Logout_POST_Should_Redirect_To_LoggedOut_When_Authenticated_And_Idp_Is_Not_Null_And_Logout_Id_Is_Not_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@sound-it-out.com",
                FirstName = "firstname",
                LastName = "lastname",
                UserName = "test@sound-it-out.com",
            };

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(JwtClaimTypes.IdentityProvider, "asdf"),
                        new Claim(JwtClaimTypes.Subject, user.Id),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            await userManager.CreateAsync(user);

            var result = await controller.Logout(new LogoutRequest { LogoutId = Guid.NewGuid().ToString() });
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Logout_POST_Should_Redirect_To_LoggedOut_When_Authenticated_And_Idp_Is_Null_And_Logout_Id_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@sound-it-out.com",
                FirstName = "firstname",
                LastName = "lastname",
                UserName = "test@sound-it-out.com",
            };

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(JwtClaimTypes.Subject, user.Id),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            await userManager.CreateAsync(user);

            var result = await controller.Logout(new LogoutRequest());
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Logout_POST_Should_Redirect_To_LoggedOut_When_Authenticated_And_Idp_Is_Null_And_Logout_Id_Is_Not_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@sound-it-out.com",
                FirstName = "firstname",
                LastName = "lastname",
                UserName = "test@sound-it-out.com",
            };

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(JwtClaimTypes.Subject, user.Id),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            await userManager.CreateAsync(user);

            var result = await controller.Logout(new LogoutRequest { LogoutId = Guid.NewGuid().ToString() });
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Logout_POST_Should_Redirect_To_LoggedOut_When_UnAuthenticated()
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

            var result = await controller.Logout(new LogoutRequest());
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void LoggedOut_GET_Should_Return_View()
        {
            var controller = BuildController(out var serviceProvider);
            var result = controller.LoggedOut();
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }
    }
}
