using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace SIO.Identity.Tests.Login.LoginController.Login.Get
{
    public class WhenUserIsAuthenticated : LoginControllerSpecification<IActionResult>
    {
        private readonly string _returnUrl = "https://localhost/mock";
        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_returnUrl);
        }

        protected override Task When()
        {
            _controller.ControllerContext = new ControllerContext()
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

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(uh => uh.IsLocalUrl(It.IsAny<string>())).Returns(true);

            _controller.Url = mockUrlHelper.Object;

            return Task.CompletedTask;
        }

        [Then]
        public void ThenShouldReturnRedirectToReturnUrl()
        {
            Result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)Result).Url.Should().Be(_returnUrl);
        }
    }
}
