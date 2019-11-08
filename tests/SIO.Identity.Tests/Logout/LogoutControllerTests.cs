using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SIO.Identity.Logout;
using SIO.Identity.Logout.Requests;
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
