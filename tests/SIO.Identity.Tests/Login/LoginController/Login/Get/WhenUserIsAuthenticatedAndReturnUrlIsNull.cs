using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace SIO.Identity.Tests.Login.LoginController.Login.Get
{
    public class WhenUserIsAuthenticatedAndReturnUrlIsNull : LoginControllerSpecification<IActionResult>
    {
        private readonly string _returnUrl = null;
        private readonly string _defaultAppUrl = "https://localhost/defaultappurl";
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

            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns(_defaultAppUrl);
            _configuration.Setup(c => c.GetSection(It.Is<string>((s) => s == "DefaultAppUrl"))).Returns(section.Object);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(uh => uh.IsLocalUrl(It.IsAny<string>())).Returns(true);

            _controller.Url = mockUrlHelper.Object;

            return Task.CompletedTask;
        }

        [Then]
        public void ThenShouldReturnRedirectToDefaultAppUrl()
        {
            Result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)Result).Url.Should().Be(_defaultAppUrl);
        }
    }
}
